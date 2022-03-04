using System;
using System.Collections.Generic;
using System.Linq;

namespace ProcGenTools.DataStructures
{
    public delegate void TreeNodeVisitor<T>(TreeNode<T> node);
    public delegate void TreeDataVisitor<T>(T data);
    public enum SearchOrder
    {
        PreOrder = 1,
        PostOrder = 2,
        BreadthFirst = 3
    }

    public class Tree<T> : ICloneable
    {
        public string Name;
        public TreeNode<T> Root;

        public object Clone()
        {
            
            Tree<T> clone = ((TreeNode<T>)Root.Clone()).tree;
            clone.Name = Name;
            return clone;
        }
    }
    public interface ITreeNode<T>
    {
        void UpdateCachedPostOrderLists();
        void UpdateCachedPreOrderLists();
        void Traverse(TreeNode<T> node, TreeNodeVisitor<T> visitor, SearchOrder searchOrder);
    }
    public class TreeNode<T> : ICloneable//: ITreeNode<T> 
    {
        private T data;
        private Guid id = Guid.NewGuid();
        public List<TreeNode<T>> children = new List<TreeNode<T>>();
        private LinkedList<TreeNode<T>> BreadthFirstVisitedList = new LinkedList<TreeNode<T>>();
        public TreeNode<T> parent;
        public Tree<T> tree;
        public int childIndex;
        public int level = 0;
        public string name;

        private LinkedList<TreeNode<T>> _cachedPreOrder;
        private LinkedList<TreeNode<T>> _cachedPostOrder;
        private void AddToPreOrderList(TreeNode<T> node)
        {
            _cachedPreOrder.AddLast(node);
        }
        private void AddToPostOrderList(TreeNode<T> node)
        {
            _cachedPostOrder.AddLast(node);
        }
        public T GetData()
        {
            return data;
        }
        public void UpdateCachedPreOrderLists()
        {
            if (_cachedPreOrder == null)
                _cachedPreOrder = new LinkedList<TreeNode<T>>();
            _cachedPreOrder.Clear();
            var preOrderDelegate = new TreeNodeVisitor<T>(AddToPreOrderList);
            this.Traverse(this, preOrderDelegate, SearchOrder.PreOrder);
            foreach(var child in this.children)
            {
                child.UpdateCachedPreOrderLists();
            }
        }
        public void UpdateCachedPostOrderLists()
        {
            if (_cachedPostOrder == null)
                _cachedPostOrder = new LinkedList<TreeNode<T>>();
            _cachedPostOrder.Clear();
            var postOrderDelegate = new TreeNodeVisitor<T>(AddToPostOrderList);
            this.Traverse(this, postOrderDelegate, SearchOrder.PostOrder);
            foreach (var child in children)
                child.UpdateCachedPostOrderLists();
        }

        public TreeNode(T data, TreeNode<T> parent, Tree<T> tree)
        {
            this.data = data;
            this.parent = parent;
            if (parent != null)
            {
                parent.children.Add(this);
                this.childIndex = parent.children.Count - 1;
                this.level = parent.level + 1;
            }

            this.tree = tree;
            if (this.tree.Root == null)
                this.tree.Root = this;
            children = new List<TreeNode<T>>();
        }

        public TreeNode<T> AddChild(T data)
        {
            var newChild = new TreeNode<T>(data, this, this.tree);
            newChild.UpdateLevel(level + 1);
            UpdateChildrenChildIndexes();
            return newChild;
        }

        public void AddChild(TreeNode<T> treeNode)
        {
            var newChild = treeNode;
            newChild.parent = this;
            children.Add(newChild);
            newChild.UpdateLevel(level + 1);
            UpdateChildrenChildIndexes();
        }
        public void Remove()
        {
            if (parent == null)
                return;
            parent.children = parent.children.Where(x => x.id != id).ToList();
            parent.UpdateChildrenChildIndexes();
        }
        public List<int> GetAddress()
        {
            List<int> Address = new List<int>();
            TreeNode<T> currentNode = this;
            while(currentNode.parent != null)
            {
                Address.Add(childIndex);
                currentNode = currentNode.parent;
            }
            return Address;
        }

        internal int _ucciChildIndex;
        internal void UpdateChildrenChildIndexes()
        {
            //only call this after removal or reordering of child
            for(_ucciChildIndex = 0; _ucciChildIndex < children.Count; _ucciChildIndex++)
            {
                children[_ucciChildIndex].childIndex = _ucciChildIndex;
            }
        }
        internal void UpdateLevel(int level)
        {
            this.level = level;
            foreach (var child in children)
            {
                child.UpdateLevel(level + 1);
            }
        }
        
        public TreeNode<T> GetChild(int i)
        {
            foreach (TreeNode<T> n in children)
                if (--i == 0)
                    return n;
            return null;
        }

        public void Traverse(TreeNode<T> node, TreeNodeVisitor<T> visitor, SearchOrder searchOrder)
        {
            //switch statement was messing up intellisense for some reason?

            if (searchOrder == SearchOrder.PostOrder)
                Traverse_PostOrder(node, visitor);
            if (searchOrder == SearchOrder.PreOrder)
                Traverse_PreOrder(node, visitor);
            if (searchOrder == SearchOrder.BreadthFirst)
                Traverse_BreadthFirst(node, visitor);
        }
        public void Traverse(TreeNode<T> node, TreeDataVisitor<T> visitor, SearchOrder searchOrder)
        {
            //switch statement was messing up intellisense for some reason?

            if (searchOrder == SearchOrder.PostOrder)
                Traverse_PostOrder(node, visitor);
            if (searchOrder == SearchOrder.PreOrder)
                Traverse_PreOrder(node, visitor);
            if (searchOrder == SearchOrder.BreadthFirst)
                Traverse_BreadthFirst(node, visitor);
        }
        private void Traverse_PreOrder(TreeNode<T> node, TreeNodeVisitor<T> visitor)
        {
            visitor(node);
            foreach (TreeNode<T> child in node.children)
                Traverse_PreOrder(child, visitor);
        }
        private void Traverse_PreOrder(TreeNode<T> node, TreeDataVisitor<T> visitor)
        {
            visitor(node.data);
            foreach (TreeNode<T> child in node.children)
                Traverse_PreOrder(child, visitor);
        }

        private void Traverse_PostOrder(TreeNode<T> node, TreeNodeVisitor<T> visitor)
        {
            foreach (TreeNode<T> child in node.children)
                Traverse_PostOrder(child, visitor);
            visitor(node);
        }
        private void Traverse_PostOrder(TreeNode<T> node, TreeDataVisitor<T> visitor)
        {
            foreach (TreeNode<T> child in node.children)
                Traverse_PostOrder(child, visitor);
            visitor(node.data);
        }

        private void Traverse_BreadthFirst(TreeNode<T> node, TreeNodeVisitor<T> visitor)
        {
            //this is not recursive
            BreadthFirstVisitedList.Clear();
            visitor(node);
            BreadthFirstVisitedList.AddLast(node);
            LinkedListNode<TreeNode<T>> ParentNode;
            ParentNode = BreadthFirstVisitedList.First;
            while(BreadthFirstVisitedList.Count > 0)
            {
                foreach(TreeNode<T> child in ParentNode.Value.children)
                {
                    visitor(child);
                    BreadthFirstVisitedList.AddLast(child);
                }
                ParentNode = ParentNode.Next;
                BreadthFirstVisitedList.RemoveFirst();
            }
        }

        private void Traverse_BreadthFirst(TreeNode<T> node, TreeDataVisitor<T> visitor)
        {
            //this is not recursive
            BreadthFirstVisitedList.Clear();
            visitor(node.data);
            BreadthFirstVisitedList.AddLast(node);
            LinkedListNode<TreeNode<T>> ParentNode;
            ParentNode = BreadthFirstVisitedList.First;
            while (BreadthFirstVisitedList.Count > 0)
            {
                foreach (TreeNode<T> child in ParentNode.Value.children)
                {
                    visitor(child.data);
                    BreadthFirstVisitedList.AddLast(child);
                }
                ParentNode = ParentNode.Next;
                BreadthFirstVisitedList.RemoveFirst();
            }
        } 

        public object Clone()
        {
            TreeCopier<T> copier = new TreeCopier<T>();
            TreeNodeVisitor <T> nodeVisitor = copier.cloneDataAndAssign;
            this.Traverse(this, nodeVisitor, SearchOrder.PreOrder);
            return copier.TreeCopy.Root;
        }

        public Tree<T> CloneAsTree()
        {
            TreeCopier<T> copier = new TreeCopier<T>();
            TreeNodeVisitor<T> nodeVisitor = copier.cloneDataAndAssign;
            this.Traverse(this, nodeVisitor, SearchOrder.PreOrder);
            return copier.TreeCopy;
        }
    }

    public static class TreeExtensions
    {
        private static TreeNode<System.Object> NextSibling(this TreeNode<System.Object> node, bool loop = true)
        {
            if (node.parent == null)
                return node;
            if (loop)
                return node.parent.children[node.childIndex + 1 % node.parent.children.Count];
            //not loop
            if (node.childIndex + 1 > node.parent.children.Count)
                return null;
            else
                return node.parent.children[node.childIndex + 1];
        }

        public static void SwapWithNextSibling<T>(this TreeNode<T> node)
        {
            if (node.parent == null)
                return;
            if (node.parent.children.Count == 1)
                return;
            int nextSiblingIndex = (node.childIndex + 1) % node.parent.children.Count;
            TreeNode<T> nextSibling = node.parent.children[nextSiblingIndex]; //18049
            int thisIndex = node.childIndex;

            node.parent.children.Swap(thisIndex, nextSiblingIndex);

            node.parent.UpdateChildrenChildIndexes();
        }

        public static void SwapWithParent<T>(this TreeNode<T> node)
        {
            if (node.parent == null)
            {
                return;
            }

            var nodesNewChildren = new List<TreeNode<T>>();
            nodesNewChildren.AddRange(node.parent.children);
            nodesNewChildren[node.childIndex] = node.parent;
            var nodesNewParent = node.parent.parent;

            if (nodesNewParent != null)
            {
                var nodesNewParentChildren = new List<TreeNode<T>>();
                nodesNewParentChildren.AddRange(nodesNewParent.children);
                nodesNewParentChildren[node.parent.childIndex] = node;
                nodesNewParent.children = nodesNewParentChildren;
            }

            node.parent.children.Clear();
            node.parent.children.AddRange(node.children);
            foreach(TreeNode<T>  oldChild in node.children)
            {
                oldChild.parent = node.parent;
            }
            node.parent.parent = node;
            node.children.Clear();
            node.children.AddRange(nodesNewChildren);
            foreach(TreeNode<T> newChild in nodesNewChildren)
            {
                newChild.parent = node;
            }
            node.parent = nodesNewParent;

            if (nodesNewParent == null)
                node.tree.Root = node;



            if (nodesNewParent != null)
            {
                nodesNewParent.UpdateChildrenChildIndexes();
            }
            else
                node.childIndex = 0;

            node.UpdateChildrenChildIndexes();


        }

        public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
            return list;
        }
    }

    public class TreeCopier<T> //where T : ICloneable
    {
        public Tree<T> TreeCopy;
        public TreeNode<T> NodeParent;
        public void cloneDataAndAssign(TreeNode<T> node)
        {
            if (!(typeof(T) is ICloneable))
            {
                throw new Exception("Tree Node can not call copy on type T=" + typeof(T).Name + " because T does not implement ICloneable.");
            }

            T newData = (T)((ICloneable)node.GetData()).Clone();
            
            if (TreeCopy == null)
            {
                TreeCopy = new Tree<T>();
            }
            if (NodeParent == null) {
                NodeParent = new TreeNode<T>(
                    newData, null, TreeCopy
                );
                NodeParent.name = node.name;
            }
            else
            {
                while (node.level != NodeParent.level + 1)
                {
                    NodeParent = NodeParent.parent;
                }
                var newChild = NodeParent.AddChild(newData);
                newChild.name = node.name;
                NodeParent = NodeParent.children.Last();
            }
        }
    }
}

