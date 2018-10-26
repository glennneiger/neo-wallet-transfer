using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transfer_gui
{
    public class RecordInfo
    {
        private string _result;
        public string Result
        {
            get { return _result; }
            set { _result = value; }
        }

        public RecordInfo(string result)
        {
            this._result = result;
        }
    }
}
