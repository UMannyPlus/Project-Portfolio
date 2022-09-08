using System;


namespace LinkedListTreeNodeDS
{
    public class Node<T>
    {
        public T Value { get; set; } //Data for the node
        public int NumChild { get; set; } //Number of children node can have
        public Node<T>[] Next { get; set; } //List of child nodes
        public Node<T> Right { get; set; } //Right node of current node
        public Node<T> Left { get; set; }   //Left node of current node
        public bool IsIntersect { get; set; } //Checks if node can intersect
        public int Index { get; set; } //Index in the node list
        
        // Instantiate random number generator.  
        private readonly Random _random = new Random();

        //Constructor to creating nodes
        public Node()
        {
            NumChild = RandomNumber(1, 4);
            IsIntersect = Convert.ToBoolean(RandomNumber(0, 2));
        }
        public Node(T value) : this ()
        {
            Value = value;
        }
        public Node(T value, int index) : this(value)
        {
            Index = index;
        }

        // Generates a random number within a range.      
        private int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        public void SetMaxChild (int currNum, int maxNum)
        {
            if (currNum >= maxNum / 2)
            {
                NumChild = RandomNumber(1, 3);
                IsIntersect = true;
            }
            else
                NumChild = RandomNumber(1, 4);

            Next = new Node<T>[NumChild];
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
