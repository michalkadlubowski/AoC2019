﻿using System;
using System.Linq;

namespace advent
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine(Calculate(100756));
            Console.WriteLine(Calculate(1969));
            Console.WriteLine(Calculate(14));
            Console.WriteLine(GetResult());
        }

        public static int GetResult()
        {
            return GetInput().Split("\r\n").Select(Int32.Parse).Sum(Calculate);
        }

        private static int Calculate(int i)
        {
            Func<int, int> calc = x => x/3 -2;
            var fuelNeededThisIter =  calc(i);
            var allNeeded = fuelNeededThisIter;
            while(calc(fuelNeededThisIter)>=0)
            {
                fuelNeededThisIter = calc(fuelNeededThisIter);
                allNeeded += fuelNeededThisIter;
            }
            return allNeeded;
        }
        public static string GetInput()
        {
return @"80590
86055
92321
131464
73326
144607
124438
72589
96471
65712
107909
141197
131589
149356
53254
54742
94498
79631
146271
72983
59687
50571
89527
72175
72089
57808
143395
74329
109760
91254
79220
131610
74277
144080
107992
93817
112252
81157
74618
55479
66420
50055
53864
75143
131285
135352
63103
133893
142154
144706
128280
92891
61066
116696
132323
74805
75160
76285
114280
124461
86605
55868
117886
57035
125382
96755
50218
123795
141878
147718
65396
76043
53013
60583
140754
86844
99086
125917
139895
60719
76850
99552
130115
76143
113743
99243
132678
130983
137577
133118
70662
102478
132083
92287
147977
60584
91031
59910
147595
145263";
        }
    }


}
