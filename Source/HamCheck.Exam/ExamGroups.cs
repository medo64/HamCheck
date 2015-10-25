using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace HamCheck {

    /// <summary>
    /// Exam group collection.
    /// </summary>
    [DebuggerDisplay("{Count} groups")]
    public class ExamGroups : IEnumerable<ExamGroup> {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        internal ExamGroups() {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly List<ExamGroup> BaseList = new List<ExamGroup>();


        /// <summary>
        /// Adds item to collection.
        /// </summary>
        /// <param name="item">Item to add.</param>
        internal void Add(ExamGroup item) {
            this.BaseList.Add(item);
        }


        #region ReadOnly

        /// <summary>
        /// Gets item at specified index.
        /// </summary>
        /// <param name="index">Index.</param>
        public ExamGroup this[int index] {
            get {
                return this.BaseList[index];
            }
        }

        /// <summary>
        /// Returns true if item is present in the collection.
        /// </summary>
        /// <param name="item">Item.</param>
        public bool Contains(ExamGroup item) {
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
        public IEnumerator<ExamGroup> GetEnumerator() {
            return this.BaseList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        #endregion

    }
}
