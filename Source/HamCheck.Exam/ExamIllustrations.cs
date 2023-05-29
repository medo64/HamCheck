using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace HamCheck {

    /// <summary>
    /// Exam illustration collection.
    /// </summary>
    [DebuggerDisplay("{Count} illustrations")]
    public class ExamIllustrations :IEnumerable<ExamIllustration> {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        internal ExamIllustrations() {
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly List<ExamIllustration> BaseList = new();


        /// <summary>
        /// Adds item to collection.
        /// </summary>
        /// <param name="item">Item to add.</param>
        internal void Add(ExamIllustration item) {
            this.BaseList.Add(item);
        }


        #region ReadOnly

        /// <summary>
        /// Gets item at specified index.
        /// </summary>
        /// <param name="index">Index.</param>
        public ExamIllustration this[int index] {
            get {
                return this.BaseList[index];
            }
        }

        /// <summary>
        /// Gets item with specified name.
        /// </summary>
        /// <param name="name">Name.</param>
        public ExamIllustration? this[string? name] {
            get {
                foreach (var item in this.BaseList) {
                    if (item.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) {
                        return item;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Returns true if item is present in the collection.
        /// </summary>
        /// <param name="item">Item.</param>
        public bool Contains(ExamIllustration item) {
            return this.BaseList.Contains(item);
        }

        /// <summary>
        /// Returns true if item is present in the collection.
        /// </summary>
        /// <param name="name">Item name.</param>
        public bool Contains(string name) {
            foreach (var item in this.BaseList) {
                if (item.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) { return true; }
            }
            return false;
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
        public IEnumerator<ExamIllustration> GetEnumerator() {
            return this.BaseList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        #endregion

    }
}
