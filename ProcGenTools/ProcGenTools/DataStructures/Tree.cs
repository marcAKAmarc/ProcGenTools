using System;
using System.Collections.Generic;

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

    public class Tree
    {
        public ITreeNode Root;
    }
    public interface ITreeNode//<T> where T : class
    {
        void UpdateCachedPostOrderLists();
        void UpdateCachedPreOrderLists();
    }
    public class TreeNode<T> : ITreeNode 
    {
        private T data;
        private Guid id = Guid.NewGuid();
        private LinkedList<TreeNode<T>> children = new LinkedList<TreeNode<T>>();
        private TreeNode<T> parent;
        private Tree tree;
        private int level = 0;

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
        public void UpdateCachedPreOrderLists()
        {
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
            _cachedPostOrder.Clear();
            var postOrderDelegate = new TreeNodeVisitor<T>(AddToPostOrderList);
            this.Traverse(this, postOrderDelegate, SearchOrder.PostOrder);
            foreach (var child in children)
                child.UpdateCachedPostOrderLists();
        }

        public TreeNode(T data, TreeNode<T> parent, Tree tree)
        {
            this.data = data;
            this.parent = parent;
            this.tree = tree;
            children = new LinkedList<TreeNode<T>>();
        }

        public void AddChild(T data)
        {
            var newChild = new TreeNode<T>(data, this, this.tree);
            children.AddLast(newChild);
            newChild.UpdateLevel(level + 1);
            tree.Root.UpdateCachedPostOrderLists();
            tree.Root.UpdateCachedPreOrderLists();
        }

        public void AddChild(TreeNode<T> treeNode)
        {
            var newChild = treeNode;
            children.AddLast(newChild);
            newChild.UpdateLevel(level + 1);
            tree.Root.UpdateCachedPostOrderLists();
            tree.Root.UpdateCachedPostOrderLists();
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

        private void Traverse_PreOrder(TreeNode<T> node, TreeNodeVisitor<T> visitor)
        {
            visitor(node);
            foreach (TreeNode<T> kid in node.children)
                Traverse_PreOrder(kid, visitor);
        }
        private void Traverse_PreOrder(TreeNode<T> node, TreeDataVisitor<T> visitor)
        {
            visitor(node.data);
            foreach (TreeNode<T> kid in node.children)
                Traverse_PreOrder(kid, visitor);
        }

        private void Traverse_PostOrder(TreeNode<T> node, TreeNodeVisitor<T> visitor)
        {
            foreach (TreeNode<T> kid in node.children)
                Traverse_PostOrder(kid, visitor);
            visitor(node);
        }
        private void Traverse_PostOrder(TreeNode<T> node, TreeDataVisitor<T> visitor)
        {
            foreach (TreeNode<T> kid in node.children)
                Traverse_PostOrder(kid, visitor);
            visitor(node.data);
        }

        private void Traverse_BreadthFirst(TreeNode<T> node, TreeNodeVisitor<T> visitor)
        {
            throw new NotImplementedException();
        }

        private void Traverse_BreadthFirst(TreeNode<T> node, TreeDataVisitor<T> visitor)
        {
            throw new NotImplementedException();
        }
    }
}

