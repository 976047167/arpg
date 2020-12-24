using System.Collections.Generic;

namespace Goap
{

    public class DataBag
    {
        Dictionary<string, object> _data = new Dictionary<string, object>();
        Dictionary<string, object[]> _datas = new Dictionary<string, object[]>();

        public void SetData(string key, object val)
        {
            if (!_data.ContainsKey(key))
                _data.Add(key, val);
            else
                _data[key] = val;
        }

        public T GetData<T>(string key)
        {
            if (_data.ContainsKey(key))
                return (T)_data[key];
            return default(T);
        }
        public void AddDatas(string key, object[] val)
        {
            if (!_datas.ContainsKey(key))
                _datas.Add(key, val);
            else
                _datas[key] = val;
        }

        public object[] GetDatas(string key)
        {
            if (_datas.ContainsKey(key))
                return _datas[key];
            return null;
        }
    }

}