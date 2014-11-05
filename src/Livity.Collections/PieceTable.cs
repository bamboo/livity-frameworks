namespace Livity.Collections
{
	public static class PieceTable
	{
		public static PieceTable<char> ForString(string s)
		{
			return ForList(ImmutableList.ForString(s));
		}

		public static PieceTable<T> ForArray<T>(T[] array)
		{
			return ForList(ImmutableList.ForArray(array));
		}

		public static PieceTable<T> ForList<T>(IImmutableList<T> list)
		{
			return new PieceTable<T>(list);
		}
	}

	/// <summary>
	/// A fully persistent implementation of the piece table data structure
	/// described in "Data Structures for Text Sequences" by Charles Crowley.
	///
	/// The data structure is optimized for the most common operations in
	/// text editing scenarios and makes supporting unlimited undo a breeze.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class PieceTable<T> : CompositeImmutableList<T>
	{
		const int PartitionThreshold = 64;

		readonly InsertionBuffer<T> _insertionBuffer;

		public PieceTable(IImmutableList<T> piece)
			: this(new[] { ListSegment.For(piece, 0) }, new InsertionBuffer<T>())
		{
		}

		PieceTable(ListSegment<T>[] listSegments, InsertionBuffer<T> insertionBuffer) : base(listSegments)
		{
			_insertionBuffer = insertionBuffer;
		}

		public PieceTable<T> Insert(int position, T value)
		{
			CheckPosition(position, Count);

			if (position == Count)
				return position == 0 ? InsertFirst(value) : Append(value);

			return new Insertion(this, position, value).Run();
		}

		public PieceTable<T> Delete(int position, int length)
		{
			CheckPosition(position, Count - 1);

			var startingPieceIndex = SegmentIndexForPosition(position);
			var startingPieceSpan = ListSegments[startingPieceIndex];
			var startingPiece = startingPieceSpan.List;

			var positionInStartingPiece = position - startingPieceSpan.Start;
			var remainingStartingPieceRange = startingPiece.GetRange(0, positionInStartingPiece);
			var newStartingPieceSpan = ListSegment.For(remainingStartingPieceRange, startingPieceSpan.Start);

			var firstIndexAfterDeletion = position + length;
			if (firstIndexAfterDeletion >= Count)
				return ReplacePieceSpans(startingPieceIndex, SegmentCount - startingPieceIndex, newStartingPieceSpan);

			var endingPieceIndex = firstIndexAfterDeletion < startingPieceSpan.End
				? startingPieceIndex
				: SegmentIndexForPosition(firstIndexAfterDeletion, startingPieceIndex);

			var endingPieceSpan = ListSegments[endingPieceIndex];
			var endingPiece = endingPieceSpan.List;

			var positionInEndingPiece = firstIndexAfterDeletion - endingPieceSpan.Start;
			var remainingEndingPieceRange = endingPiece.GetRange(positionInEndingPiece, endingPiece.Count - positionInEndingPiece);
			var newEndingPieceSpan = ListSegment.For(remainingEndingPieceRange, newStartingPieceSpan.End);

			return ReplacePieceSpans(
				startingPieceIndex,
				endingPieceIndex - startingPieceIndex + 1,
				newStartingPieceSpan, newEndingPieceSpan);
		}

		PieceTable<T> InsertFirst(T value)
		{
			return NewPieceTableWith(NewSpanWith(value, 0));
		}

		PieceTable<T> Append(T value)
		{
			return AppendableList.IsAppendable(LastList)
				? AppendTo(LastIndex, value)
				: AppendNew(value);
		}

		PieceTable<T> AppendTo(int pieceIndex, T value)
		{
			var pieceSpan = ListSegments[pieceIndex];
			var appendablePiece = (IAppendableList<T>)pieceSpan.List;
			var appended = ListSegment.For(appendablePiece.Append(value), pieceSpan.Start);
			return ReplacePieceSpan(pieceIndex, appended);
		}

		PieceTable<T> AppendNew(T value)
		{
			var newPieceSpans = ListSegments.Append(NewSpanWith(value, Count));
			return NewPieceTableWith(newPieceSpans);
		}

		ListSegment<T> NewSpanWith(T value, int start)
		{
			return ListSegment.For(NewPieceWith(value), start);
		}

		IImmutableList<T> NewPieceWith(T value)
		{
			return _insertionBuffer.Insert(value);
		}

		PieceTable<T> NewPieceTableWith(params ListSegment<T>[] newListSegments)
		{
			var newPieceTable = new PieceTable<T>(newListSegments, _insertionBuffer);
			return newPieceTable.SegmentCount >= PartitionThreshold
				? PartitionedPieceTableFor(newPieceTable)
				: newPieceTable;
		}

		static PieceTable<T> PartitionedPieceTableFor(PieceTable<T> p)
		{
			var pivot = p.Count / 2;
			var parts = p.SplitAt(pivot);
			var first = ListSegment.For(parts.First, 0);
			var second = ListSegment.For(parts.Second, pivot);
			return new PieceTable<T>(new[] {first, second}, new InsertionBuffer<T>());
		}

		static void UpdateSpans(ListSegment<T>[] spans, int startingIndex)
		{
			for (var i = startingIndex; i < spans.Length; ++i)
			{
				var span = spans[i];
				spans[i] = ListSegment.For(span.List, spans[i - 1].End);
			}
		}

		PieceTable<T> ReplacePieceSpan(int pieceIndex, params ListSegment<T>[] replacementPieces)
		{
			return ReplacePieceSpans(pieceIndex, 1, replacementPieces);
		}

		PieceTable<T> ReplacePieceSpans(int pieceIndex, int count, params ListSegment<T>[] replacementPieces)
		{
			var nonEmptySpans = ListSegment.NonEmptySegmentsFrom(replacementPieces);
			var newPieceSpans = ListSegments.ReplaceRange(pieceIndex, count, nonEmptySpans);
			UpdateSpans(newPieceSpans, pieceIndex + nonEmptySpans.Length);
			return NewPieceTableWith(newPieceSpans);
		}

		struct Insertion
		{
			readonly PieceTable<T> _table;
			readonly int _position;
			readonly T _value;
			readonly int _pieceIndex;
			readonly ListSegment<T>[] _listSegments;
			readonly ListSegment<T> _listSegment;
			readonly IImmutableList<T> _piece;

			public Insertion(PieceTable<T> table, int position, T value)
			{
				_pieceIndex = table.SegmentIndexForPosition(position);
				_table = table;
				_position = position;
				_value = value;
				_listSegments = _table.ListSegments;
				_listSegment = _listSegments[_pieceIndex];
				_piece = _listSegment.List;
			}

			public PieceTable<T> Run()
			{
				return CanInsertByAppendingToPreviousPiece()
					? InsertByAppending()
					: InsertBySplitting();
			}

			bool CanInsertByAppendingToPreviousPiece()
			{
				return InsertingAtTheBeginning
					&& HasPreviousPiece
					&& PreviousPieceIsAppendable;
			}

			bool InsertingAtTheBeginning
			{
				get { return _position == _listSegment.Start; }
			}

			bool HasPreviousPiece
			{
				get { return _pieceIndex > 0; }
			}

			bool PreviousPieceIsAppendable
			{
				get { return AppendableList.IsAppendable(Previous.List); }
			}

			ListSegment<T> Previous
			{
				get { return _listSegments[PreviousIndex]; }
			}

			int PreviousIndex
			{
				get { return _pieceIndex - 1; }
			}

			PieceTable<T> InsertByAppending()
			{
				return _table.AppendTo(PreviousIndex, _value);
			}

			PieceTable<T> InsertBySplitting()
			{
				var pieceStart = _listSegment.Start;
				var positionInPiece = _position - pieceStart;
				var parts = _piece.SplitAt(positionInPiece);
				var replacementPieces = new[]
				{
					ListSegment.For(parts.First, pieceStart),
					_table.NewSpanWith(_value, _position),
					ListSegment.For(parts.Second, _position + 1)
				};
				return _table.ReplacePieceSpan(_pieceIndex, replacementPieces);
			}
		}
	}
}