using System;
using System.Collections.Generic;


namespace LinkedListTreeNodeDS
{
    class LinkedListTree<T>
    {
        /*
         * Generating The List
         * 1st Get length from input
         * 2nd Generate the root node
         * - Get the number of children the node can have
         * 3rd Generate the children nodes, then connect nodes to root node
         * 4th Check if nodes are intersectionable and count up the number
         * of child nodes, the new nodes need.
         * Subtract the number of nodes based on the intersection.
         * 5th Generate new nodes, and connect leftmost nodes children
         * - if next node intersects grab the rightmost child of the node to the left and connect.
         * 6th Repeat until halfway, then beign halfing the number of nodes needed
         * 7th Once it gets to second to last index, change all but 
         * left most node to intersect
         * 8th Generate the final node, and connect it to the nodes
         */
        private Node<T> _currentRight; //Gets the right node and stores it for sibling
        private List<Node<T>>[] _nodeArr; //An array of list of nodes that will store the nodes of each index
        private int _nodeListLength; //The length of node list
        public int MaxNodeCount { get; set; } //Max number of nodes in tree
        public T Value { get; set; }

        public LinkedListTree(T value)
        {
            Value = value;
        }



        public Node<T> GenerateNodeList(int listLength)
        {
            _nodeListLength = listLength;
            //Instantiated the node array
            _nodeArr = new List<Node<T>>[_nodeListLength];

            //instantiated each list in the array
            for (int i = 0; i < _nodeListLength; i++)
            {
                _nodeArr[i] = new List<Node<T>>();
            }

            return GenerateNodeList(new Node<T>(Value));
        }
        private Node<T> GenerateNodeList(Node<T> _node) //This will start with the root node and then traverse the list length
        {
            //Set the defaults for the root node
            _node.IsIntersect = false;
            _node.Index = 0;
            _node.NumChild = 3;
            _node.Next = new Node<T>[_node.NumChild];
            _nodeArr[0].Add(_node);
            MaxNodeCount++;

            PopulateListArray();
            MakeConnections();
            //PrintNodeData(_node);

            return _node;
        }

        //Populate the list rray with nodes at each index
        private void PopulateListArray()
        {
            int numChild = 0;
            for (int i = 0; i < _nodeArr.Length; i++)
            {
                for (int j = 0; j < _nodeArr[i].Count; j++)
                {
                    Node<T> temp = _nodeArr[i][j];

                    temp.SetMaxChild(i, _nodeListLength);//Set the number of childs

                    //Check if the node is the leftmost node at the current index and set IsIntersect to false
                    if (temp == _nodeArr[i][0] && temp.IsIntersect)
                    {
                        temp.IsIntersect = false;
                        _nodeArr[i][0] = temp;
                    }

                    //----Check for specialty cases----
                    //Second to last index
                    if (i == _nodeListLength - 2)
                    {
                        temp.NumChild = 1;                      //Set the child to 1
                        temp.Next = new Node<T>[1];             //Make the child array 1

                        if (temp != _nodeArr[_nodeListLength - 2][0])
                        {
                            //Check if its not the leftmost node
                            temp.IsIntersect = true;            //Make sure the node intersects
                            _nodeArr[i][j] = temp;              //Set temp to the actual node
                        }
                    }

                    //Get the number of child nodes
                    numChild = GetNodeCount(temp);
                    for (int k = 0; k < numChild; k++)
                    {
                        if (i == _nodeListLength - 1)
                            break;

                        //This is a testcase for when an integer is given
                        if (Value.GetType() == typeof(int))
                        {
                            var value = Convert.ToInt32(Value);
                            Value = (T)(object)++value;
                        }

                        _nodeArr[i + 1].Add(new Node<T>(Value, i + 1));
                        MaxNodeCount++;
                    }
                }
            }
        }

        private void MakeConnections()
        {
            //Traverse through node Array List
            for (int i = 0; i < _nodeArr.Length; i++)
            {
                for (int j = 0; j < _nodeArr[i].Count; j++)
                {
                    AssignSiblings(_nodeArr[i][j]);
                }
            }
            for (int i = 0; i < _nodeArr.Length; i++)
            {
                for (int j = 0; j < _nodeArr[i].Count; j++)
                {
                    AssignChildren(_nodeArr[i][j]);
                }
            }
        }

        private int GetNodeCount(Node<T> _node)
        {
            //Get the number of children the node can have
            int numOfNodes = _node.NumChild;

            //Checks if the node can intersect
            if (_node.IsIntersect)
                numOfNodes--;   //if the node intersects remove 1 from list

            return numOfNodes;
        }

        private void AssignSiblings(Node<T> node)
        {
            for (int i = 0; i < _nodeArr.Length; i++)
            {
                for (int j = 0; j < _nodeArr[i].Count; j++)
                {
                    Node<T> temp = _nodeArr[i][j];

                    if (temp == node)
                    {
                        if (j - 1 == -1)
                            node.Left = node;               //Leftmost Node
                        else
                            node.Left = _nodeArr[i][j - 1];

                        if (j + 1 == _nodeArr[i].Count)
                            node.Right = node;              //Rightmost Node
                        else
                            node.Right = _nodeArr[i][j + 1];
                    }
                }
            }
        }
        //Assigning the child nodes
        private void AssignChildren(Node<T> node)
        {
            //Sets the special case for the EndRoot
            if (node.Index == _nodeListLength - 1)
            {
                node.NumChild = 1;
                node.Next = new Node<T>[1];
                node.Next[0] = node;
            }
            else
            {
                //Sets the currentRight node
                if (node.Index != _nodeListLength - 1 && (_currentRight == null || _currentRight.Index == node.Index))
                    _currentRight = _nodeArr[node.Index + 1][0];

                //Assigns the child node
                for (int i = 0; i < node.Next.Length; i++)
                {
                    node.Next[i] = _currentRight;

                    //Iterate currentRight node
                    if (node.Right.IsIntersect && i == node.Next.Length - 1)
                    {
                        //Do nothing
                    }
                    else
                    {
                        _currentRight = _currentRight.Right;
                    }
                    
                }    
            }
        }

        public override string ToString()
        {
            string temp = "";

            foreach (var oneD in _nodeArr)
            {
                foreach (var el in oneD)
                {
                    temp += el + " ";
                }
                temp += "\n";
            }
            return temp;
        }

        public void PrintNodeData(Node <T> node)
        {
            Console.WriteLine("Current Node: " + node.ToString());
            Console.WriteLine("Right Node: " + node.Right.ToString());
            Console.WriteLine("Left Node: " + node.Left.ToString());
            Console.WriteLine("Number of Children: " + node.Next.Length);
            Console.Write("Child Nodes: ");
            foreach(var el in node.Next)
            {
                Console.Write(el.ToString() + " ");
            }
            Console.WriteLine("\n");

            if (node.Next[0] != node)
                foreach(var el in node.Next)
                {
                    PrintNodeData(el);
                }
        }




    }
}
