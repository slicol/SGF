using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SGF.MvvmLite
{
    public class MvvmViewModel: MvvmBinding
    {
        protected MvvmModel m_model;

        public MvvmViewModel(MvvmModel model)
        {
            m_model = model;
        }
    }
}
