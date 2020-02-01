using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcGenTools.DataStructures
{
    public class ConnectedGraph<T>
    {
        public List<iConnectedGraphNode<T>> Nodes;
    }

    public interface iConnectedGraphNode<T>
    {
        List<iConnectedGraphNode<T>> GetChildren();
        T GetData();
        void SetData(T data);
        Guid GetId();
        void Connect(iConnectedGraphNode<T> toAndFrom);
        void ConnectTo(iConnectedGraphNode<T> to);
        void ConnectFrom(iConnectedGraphNode<T> from);
    }
    public class ConnectedGraphNode<T>:iConnectedGraphNode<T>
    {
        private Guid id = new Guid();
        private List<iConnectedGraphNode<T>> children;
        private T data;
        public List<iConnectedGraphNode<T>> GetChildren()
        {
            return children;
        }
        public T GetData()
        {
            return data;
        }
        public void SetData(T _data)
        {
            data = _data;
        }
        public Guid GetId()
        {
            return id;
        }
        public void ConnectTo(iConnectedGraphNode<T> to)
        {
            if (!children.Any(x => x.GetId() == to.GetId()))
                children.Add(to);
        }
        public void ConnectFrom(iConnectedGraphNode<T> from)
        {
            from.ConnectTo(this);
        }

        public void Connect(iConnectedGraphNode<T> toAndFrom)
        {
            ConnectTo(toAndFrom);
            toAndFrom.ConnectTo(toAndFrom);
        }
    }
}
