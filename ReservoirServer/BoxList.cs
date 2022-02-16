using ReservoirServer.Enterty;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReservoirServer
{
    class BoxList
    {
        ConcurrentDictionary<string, Box> dic;
        Mutex locker = new Mutex();

        int _maxclients = 100;
        public int MaxClients => _maxclients;

        private byte maxcore = 0;
        private int _threshold = 100;

        public Action<Action<KeyValuePair<string, Box>>> SmartTraverseMethod
        {
            get
            {
                if (maxcore < 0)
                    return SerialTraverse;
                else
                {
                    if(Count > _threshold)
                        return ParallelTraverse;
                    else
                        return SerialTraverse;
                }
            }
        }

        public BoxList(int maxclients,byte maxcore)
        {
            _maxclients = maxclients;
            this.maxcore = maxcore;
            _threshold = 1000;  //TODO: Read from ini
            dic = new ConcurrentDictionary<string, Box>(10, _maxclients);
        }

        private int _count = 0;
        public int Count
        {
            get
            {
                locker.WaitOne();
                int rt = _count;
                locker.ReleaseMutex();
                return rt;
            }
        }

        public Box this[string id]
        {
            get
            {
                bool succ = dic.TryGetValue(id, out Box rt);
                return succ ? rt : null;
            }
        }

        public bool Remove(string id)
        {
            bool rt = dic.TryRemove(id, out _);
            if (rt)
            {
                locker.WaitOne();
                _count--;
                locker.ReleaseMutex();
            }
            return rt;
        }

        public bool AddNew(Box box)
        {
            locker.WaitOne();
            bool rt;
            if (_count >= MaxClients)
            {
                locker.ReleaseMutex();
                return false;
            }
            else
            {
                rt = dic.TryAdd(box.ID, box);
                if (rt)
                    _count++;
                locker.ReleaseMutex();

            }
            return rt;
            
        }

        public void SerialTraverse(Action<KeyValuePair<string, Box>> func)
        {
            foreach (var pair in dic)
                func(pair);
        }

        public void ParallelTraverse(Action<KeyValuePair<string, Box>> func)
        {
            if(maxcore > 0)
            {
                ParallelOptions opt = new ParallelOptions() { MaxDegreeOfParallelism = maxcore };
                Parallel.ForEach(dic, opt, func);
            }
            else
            {
                Parallel.ForEach(dic, func);
            }    
        }

        public void ParallelTraverse(Action<KeyValuePair<string, Box>> func,byte coreusage)
        {
            if (coreusage > 0)
            {
                ParallelOptions opt = new ParallelOptions() { MaxDegreeOfParallelism = coreusage };
                Parallel.ForEach(dic, opt, func);
            }
            else
            {
                Parallel.ForEach(dic, func);
            }
        }


    }
}
