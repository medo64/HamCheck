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
        /// <param name="name">Illustration name</param>
        /// <param name="pictureBytes">Illustration bytes</param>
        /// <exception cref="System.ArgumentNullException">Name cannot be null. -or- Illustration cannot be null.</exception>
        internal ExamIllustration(string name, byte[] pictureBytes) {
            if (name == null) { throw new ArgumentNullException(nameof(name), "Name cannot be null."); }
            if (pictureBytes == null) { throw new ArgumentNullException(nameof(pictureBytes), "Picture cannot be null."); }

            this.Name = name;
            this.PictureBytes = pictureBytes;
        }


        /// <summary>
        /// Gets illustration name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets illustration bitmap.
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] PictureBytes { get; private set; }
#pragma warning restore CA1819 // Properties should not return arrays

        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) {
            var other = obj as ExamIllustration;
            return (other != null) && (this.Name.Equals(other.Name, StringComparison.Ordinal));
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
