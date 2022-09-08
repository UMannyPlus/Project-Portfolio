using System;

namespace LinkedListTreeNodeDS
{
    class Program
    {
        static void Main(string[] args)
        {
            int num = 0;
            var list = new LinkedListTree<int>(++num);
            list.GenerateNodeList(8);
            Console.WriteLine(list);
            Console.WriteLine("The number of max nodes: " + list.MaxNodeCount.ToString());
        }
    }
}
