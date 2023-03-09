﻿using System.Collections.Generic;

namespace Entity
{
    public class Road
    {
        public Road(int id, SortedList<int, Lane> lanes)
        {
            Id = id;
            Lanes = lanes;
        }

        public int Id { get; set; }


        public SortedList<int, Lane> Lanes { get; set; }
    }
}
