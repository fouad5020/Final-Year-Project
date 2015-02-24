using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Smart_Schedule
{
    public class Student
    {
        private Int32 stuID;

        public Student Left;
        public Student Right;

        public int leftHeight;
        public int rightHeight;

        public Student()
        {
            stuID = 0;
            Left = null;
            Right = null;
            leftHeight = rightHeight = 0;
        }

        public Student(Int32 sID)
        {
            stuID = sID;
            Left = null;
            Right = null;
            leftHeight = rightHeight = 0;
        }

        public Int32 getSID()
        { return stuID; }

        public void setSID(int id)
        { stuID = id; }
    }

    public class StudentTree
    {
        private Student root;

        public StudentTree()
        {
            root = null;
        }

        public StudentTree(Int32 SID)
        {
            root = new Student(SID);
        }
        public bool Add(Int32 s)
        {
            Student node = new Student(s);
            if (root == null)
            {
                root = node;
                return true;
            }

            else
            {
                return Insert(node, ref root);
            }
            //return false;
        }

        public Student getRoot()
        {
            return root;
        }

        protected bool Insert(Student node, ref Student tree)
        {
            if (tree == null)
            {
                tree = node;
                return true;
            }

            else
            {
                if (node.getSID() < tree.getSID())
                {
                    if (Insert(node, ref tree.Left) == false)
                        return false;
                }

                else if (node.getSID() > tree.getSID())
                {
                    if (Insert(node, ref tree.Right) == false)
                        return false;
                }

                else
                    return false;

                // Balancing the tree
                tree.leftHeight = calcHeight(ref tree.Left);
                tree.rightHeight = calcHeight(ref tree.Right);
                if (checkBalance(tree))
                {// if balance of a node is violated 
                    if (node.getSID() > tree.getSID())
                        BalanceRight(node, ref tree);

                    if (node.getSID() < tree.getSID())
                        BalanceLeft(node, ref tree);
                }
                return true;
            }
        }

        protected int calcHeight(ref Student node)
        {
            if (node != null)
            {// if there exists a node, then the maximum of the 2 heights of the node is returned with an increment
                return Math.Max(node.leftHeight, node.rightHeight) + 1;
            }

            else
                return 0;       // if the pointer is null, then the height is 0
        }
        public TreeNode getStudentTree()
        {
            TreeNode tr = new TreeNode();
            Student node = root;
            tr = traverse(ref root, ref tr);
            return tr;
        }

        public List<Int32> getStudentTreeList()
        {
            List<Int32> list = new List<Int32>();
            Student node = root;
            return list;
        }

        protected List<Int32> traverseList(ref Student node, ref List<Int32> list)
        {
            if (node != null)
            {
                traverseList(ref node.Left, ref list);
                list.Add(node.getSID());
                traverseList(ref node.Right, ref list);
            }
            return list;
        }

        protected TreeNode traverse(ref Student node, ref TreeNode tr)
        {
            TreeNode l = new TreeNode();
            TreeNode r = new TreeNode();
            TreeNode tn = null;
            if (node != null)
            {
                l = traverse(ref node.Left, ref tr);
                //tr.Nodes.Add(node.getSID().ToString());
                r = traverse(ref node.Right, ref tr);
                if (l == null)
                    l = new TreeNode("");
                if (r == null)
                    r = new TreeNode("");
                TreeNode[] arr = new TreeNode[] { l, r };
                tn = new TreeNode(node.getSID().ToString(), arr);
            }
            
            return tn;
        }

        protected bool checkBalance(Student node)
        {
            if (((node.rightHeight - node.leftHeight) >= 2) || ((node.rightHeight - node.leftHeight) <= -2))
                return true;

            else
                return false;
        }

        protected void BalanceRight(Student node, ref Student tree)
        {
            if (node.getSID() > tree.Right.getSID())
            {// Right-Right 
                int id = tree.getSID();
                tree.setSID(tree.Right.getSID());   // swap the id's istead of whole data structures
                tree.Right.setSID(id);              // swapping
                Student temp = tree.Right;          // a temporary pointer to catch the parent node's right
                tree.Right = temp.Right;            // parent right node is free, placing node on parent's right
                temp.Right = temp.Left;             // creating parent's left node's right
                temp.Left = tree.Left;              // creating parent's left node's left
                tree.Left = temp;                   // placing the created node on parent's left
            }

            if (node.getSID() < tree.Right.getSID())
            {// Right-Left 
                int id = tree.getSID();
                tree.setSID(tree.Right.Left.getSID());
                tree.Right.Left.setSID(id);
                Student temp = tree.Right.Left;
                tree.Right.Left = temp.Right;
                temp.Right = temp.Left;
                temp.Left = tree.Left;
                tree.Left = temp;
            }

            //updating the balance factors after successfull balancing
            tree.Left.leftHeight = calcHeight(ref tree.Left.Left);
            tree.Left.rightHeight = calcHeight(ref tree.Left.Right);
            tree.Right.leftHeight = calcHeight(ref tree.Right.Left);
            tree.Right.rightHeight = calcHeight(ref tree.Right.Right);
            tree.leftHeight = calcHeight(ref tree.Left);
            tree.rightHeight = calcHeight(ref tree.Right);
        }

        protected void BalanceLeft(Student node, ref Student tree)
        {
            if (node.getSID() < tree.Left.getSID())
            {// Left-Left 
            }

            if (node.getSID() > tree.Left.getSID())
            {// Left-Right 
            }
        }

        public bool detectStudent(Int32 id)
        {
            Student temp = root;
            bool clash = false;
            while (temp != null)
            {
                if (temp.getSID() == id)
                    return true;

                else if (temp.getSID() > id)
                    temp = temp.Left;

                else
                    temp = temp.Right;
            }
            return clash;
        }
    }
}
