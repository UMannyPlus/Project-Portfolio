using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkedListTreeNodeDS
{
    public class TestClass
    {
        public static int num = 0;

        public TestClass()
        {
            num++;
        }


        public override string ToString()
        {
            return num.ToString();
        }
    }
}
