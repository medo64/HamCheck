using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace HamCheck {

    /// <summary>
    /// Exam answer collection.
    /// </summary>
    [DebuggerDisplay("{Count} elements")]
    public class ExamElements : IEnumerable<ExamElement> {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        private ExamElements() {
        }


        private static ExamElements _all;
        /// <summary>
        /// Gets all elements
        /// </summary>
        public static ExamElements All {
            get {
                if (_all == null) {
                    _all = new ExamElements();
                    string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                    foreach (var resourceName in resourceNames) {
                        if (resourceName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) {
                            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
                                _all.Add(ExamElement.Load(stream));
                            }
                        }
                    }
                }
                return _all;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private readonly List<ExamElement> BaseList = new List<ExamElement>();


        /// <summary>
        /// Adds item to collection.
        /// </summary>
        /// <param name="item">Item to add.</param>
        internal void Add(ExamElement item) {
            this.BaseList.Add(item);
        }


        #region ReadOnly

        /// <summary>
        /// Gets item at specified index.
        /// </summary>
        /// <param name="index">Index.</param>
        public ExamElement this[int index] {
            get {
                return this.BaseList[index];
            }
        }

        /// <summary>
        /// Returns true if item is present in the collection.
        /// </summary>
        /// <param name="item">Item.</param>
        public bool Contains(ExamElement item) {
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
        public IEnumerator<ExamElement> GetEnumerator() {
            return this.BaseList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        #endregion

    }
}
