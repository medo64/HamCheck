using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;

namespace HamCheck {

    /// <summary>
    /// Exam illustration.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class ExamIllustration {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="code">Question code.</param>
        /// <param name="text">Question text.</param>
        /// <param name="illustration">Question illustration.</param>
        /// <param name="fccReference">FCC reference text.</param>
        /// <exception cref="System.ArgumentNullException">Name cannot be null. -or- Illustration cannot be null.</exception>
        internal ExamIllustration(string name, Bitmap picture) {
            if (name == null) { throw new ArgumentNullException("name", "Name cannot be null."); }
            if (picture == null) { throw new ArgumentNullException("picture", "Picture cannot be null."); }

            this.Name = name;
            this.Picture = picture;
        }


        /// <summary>
        /// Gets illustration name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets illustration bitmap.
        /// </summary>
        public Bitmap Picture { get; private set; }

        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) {
            var other = obj as ExamIllustration;
            return (other != null) && (this.Name.Equals(other.Name));
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        public override int GetHashCode() {
            return this.Name.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString() {
            return this.Name;
        }

        #endregion

    }
}
