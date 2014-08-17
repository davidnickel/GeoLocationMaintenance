using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXApplication1
{
    public class ValidatableFileInfo : IDataErrorInfo
    {
        private Dictionary<string, string> _propertyErrors;


        public ValidatableFileInfo(FileInfo fileInfo)
        {
            FileInfo = fileInfo;

            _propertyErrors = new Dictionary<string, string>();
            _propertyErrors.Add("Name", "");
        }

        public string Name
        {
            get
            {
                return FileInfo != null ? FileInfo.Name : String.Empty;
            }
        }

        public FileInfo FileInfo
        {
            get;
            set;
        }

        public void SetColumnError(string elem, string error)
        {
            if (_propertyErrors.ContainsKey(elem))
            {
                if ((string)_propertyErrors[elem] == error) return;
                _propertyErrors[elem] = error;
            }
        }

        public string GetColumnError(string elem)
        {
            if (_propertyErrors.ContainsKey(elem))
                return (string)_propertyErrors[elem];
            else
                return "";
        }

        public void ClearErrors()
        {
            foreach (string elem in _propertyErrors.Keys)
            {
                SetColumnError(elem, "");
            }
        }

        public string this[string columnName]
        {
            get
            {
                return GetColumnError(columnName);
            }
        }

        public string Error
        {
            get
            {
                return GetColumnError("Name");
            }

        }
    }
}
