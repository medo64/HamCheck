using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace HamCheck {

    /// <summary>
    /// Exam answer collection.
    /// </summary>
    [DebuggerDisplay("{Count} answers")]
    public class ExamAnswers : IEnumerable<ExamAnswer> {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        internal ExamAnswers() {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly List<ExamAnswer> BaseList = new List<ExamAnswer>();


        /// <summary>
        /// Adds item to collection.
        /// </summary>
        /// <param name="item">Item to add.</param>
        internal void Add(ExamAnswer item) {
            this.BaseList.Add(item);
        }


        #region ReadOnly

        /// <summary>
        /// Gets item at specified index.
        /// </summary>
        /// <param name="index">Index.</param>
        public ExamAnswer this[int index] {
            get {
                return this.BaseList[index];
            }
        }

        /// <summary>
        /// Returns true if item is present in the collection.
        /// </summary>
        /// <param name="item">Item.</param>
        public bool Contains(ExamAnswer item) {
            return this.BaseList.Contains(item);
        }

        /// <summary>
        /// Gets number of items in collection.
        /// </summary>
        public int Count {
            get { return this.BaseList.Count; }
        }

        /// <summary>
        /// Returns enumerator for collection.
        /// </summary>
        public IEnumerator<ExamAnswer> GetEnumerator() {
            return this.BaseList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        #endregion

    }
}
