using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace HamCheck {

    /// <summary>
    /// Exam subelement collection.
    /// </summary>
    [DebuggerDisplay("{Count} subelements")]
    public class ExamSubelements : IEnumerable<ExamSubelement> {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        internal ExamSubelements() {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly List<ExamSubelement> BaseList = new List<ExamSubelement>();


        /// <summary>
        /// Adds item to collection.
        /// </summary>
        /// <param name="item">Item to add.</param>
        internal void Add(ExamSubelement item) {
            this.BaseList.Add(item);
        }


        #region ReadOnly

        /// <summary>
        /// Gets item at specified index.
        /// </summary>
        /// <param name="index">Index.</param>
        public ExamSubelement this[int index] {
            get {
                return this.BaseList[index];
            }
        }

        /// <summary>
        /// Returns true if item is present in the collection.
        /// </summary>
        /// <param name="item">Item.</param>
        public bool Contains(ExamSubelement item) {
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
        public IEnumerator<ExamSubelement> GetEnumerator() {
            return this.BaseList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        #endregion

    }
}
