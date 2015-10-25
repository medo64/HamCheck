using System;

namespace HamCheck {
    internal class ExamElementEventArgs : EventArgs {

        public ExamElementEventArgs(ExamElement element) {
            this.Element = element;
            this.Type = ExamType.None;
        }

        public ExamElementEventArgs(ExamElement element, ExamType type) {
            this.Element = element;
            this.Type = type;
        }


        public ExamElement Element { get; }
        public ExamType Type { get; }

    }
}
