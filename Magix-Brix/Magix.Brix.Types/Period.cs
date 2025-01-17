﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2011 - Ra-Software, Inc. - thomas.hansen@winergyinc.com
 * Magix is licensed as GPLv3, or Commercially for Proprietary Projects through Ra-Software.
 */

using System;

namespace Magix.Brix.Types
{
    /**
     * Level3: Helper type for having a "period" which consists not only of a DateTime
     * but also a length. Combination of DateTime and TimeSpan. Struct is a natural 
     * immutable class and hence 100% thread safe in all regards. Struct also contains
     * basic algebraic operations, the ones which makes sense like the OR, AND and XOR
     * operations
     */
    public struct Period
    {
        private readonly DateTime _start;
        private readonly TimeSpan _length;

        public static readonly Period Empty;

        static Period()
        {
            Empty = new Period();
        }

        /**
         * Level3: CTOR taking start and end. Will throw an exception if end is less then
         * or equal to start.
         */
        public Period(DateTime start, DateTime end)
        {
            if (start >= end)
                throw new Exception("Can't have a period with negative length");

            _start = start;
            _length = end - start;
        }

        /**
         * Level3: CTOR taking a start and a length.
         */
        public Period(DateTime start, TimeSpan length)
        {
            _start = start;
            _length = length;
        }

        /**
         * Level3: Returns the start DateTime of the period
         */
        public DateTime Start
        {
            get { return _start; }
        }

        /**
         * Level3: Returns the end DateTime of the period
         */
        public DateTime End
        {
            get { return _start + _length; }
        }

        /**
         * Level3: Returns the length TimeSpan of the period
         */
        public TimeSpan Length
        {
            get { return _length; }
        }

        /**
         * Level3: Will return true if given periods are in any ways overlapping. E.g.
         * the left period isn't ending before the right one opens
         */
        public static bool Intersects(Period left, Period right)
        {
            return Equals(left, right) ||
                left.Start == right.Start ||
                left.End == right.End ||
                (left.End > right.Start && left.End < right.End) ||
                (left.Start > right.Start && left.Start < right.End) ||
                (left.Start < right.Start && left.End > right.End) ||
                (right.Start < left.Start && right.End > left.End);
        }

        /**
         * Level3: Will return true if the given two periods in any ways "continues"
         * and together creates a perfect longer period
         */
        public static bool Continues(Period left, Period right)
        {
            return left.End == right.Start || left.Start == right.End;
        }

        /**
         * Level3: Returns true if first period starts earlier then second period
         */
        public static bool Earlier(Period left, Period right)
        {
            return left.Start < right.Start;
        }

        /**
         * Level3: Returns true if first period starts later then second period
         */
        public static bool Later(Period left, Period right)
        {
            return left.Start > right.Start;
        }

        /**
         * Level3: Returns true if first period is a longer period then second period
         */
        public static bool Longer(Period left, Period right)
        {
            return left.Length > right.Length;
        }

        /**
         * Level3: Returns true if first period is a smaller period then second period
         */
        public static bool Shorter(Period left, Period right)
        {
            return left.Length < right.Length;
        }

        /**
         * Level3: Returns true if the given periods have the same start and end values.
         * Periods are equal.
         */
        public static bool Equals(Period left, Period right)
        {
            return left.Start == right.Start && left.End == right.End;
        }

        /**
         * Level3: Returns true is the first period is "within" the second period. Meaning
         * the entire left period is completely within the right period
         */
        public static bool Subset(Period left, Period right)
        {
            return left.Start > right.Start && left.End < right.End;
        }

        /**
         * Level3: Returns true is the second period is "within" the first period
         */
        public static bool Superset(Period left, Period right)
        {
            return Subset(right, left);
        }

        /**
         * Level3: "Merges" two periods into one. Periods MUST be either overlapping or 
         * continuing periods otherwise an exception will be thrown
         */
        public static Period OR(Period left, Period right)
        {
            if (!Intersects(left, right) && !Continues(left, right))
                throw new Exception("Can't logically OR two periods that doesn't 'touch' eachother");
            return new Period(left.Start < right.Start ? left.Start : right.Start, left.End > right.End ? left.End : right.End);
        }

        /**
         * Level3: Returns a new period that is the overlapping parts of the given two 
         * periods. Periods MUST overlap otherwise an exception will be thrown.
         */
        public static Period AND(Period left, Period right)
        {
            if (!Intersects(left, right))
                throw new Exception("Can't logically AND two periods that doesn't overlap");
            return new Period(left.Start > right.Start ? left.Start : right.Start, left.End < right.End ? left.End : right.End);
        }

        /**
         * Level3: Returns the XOR values of two periods. This logically is two periods 
         * which are the places that only one of them exists
         */
        public static Tuple<Period, Period> XOR(Period left, Period right)
        {
            if (Equals(left, right))
                throw new Exception("Can't XOR two periods that are equal");
            if (left.Start == right.Start || left.End == right.End)
                throw new Exception("Can't XOR two periods that will not somehow create two periods");
            if (Subset(left, right))
            {
                // Left is subset of right
                return new Tuple<Period, Period>(
                    new Period(right.Start, left.Start),
                    new Period(left.End, right.End));
            }
            if (Subset(right, left))
            {
                // Right is subset of right
                return new Tuple<Period, Period>(
                    new Period(left.Start, right.Start),
                    new Period(right.End, left.End));
            }
            if (Intersects(left, right))
            {
                // Periods are overlapping
                return new Tuple<Period, Period>(
                    new Period(
                        left.Start < right.Start ? left.Start : right.Start, 
                        left.Start > right.Start ? left.Start : right.Start),
                    new Period(
                        left.End < right.End ? left.End : right.End,
                        left.End > right.End ? left.End : right.End));
            }
            // Periods are already "naturally" XORING eachother...
            return new Tuple<Period, Period>(left, right);
        }

        /**
         * Level3: Returns false if periods are the same
         */
        public static bool operator != (Period left, Period right)
        {
            return !(left == right);
        }

        // TODO: Use Equals method instead ...
        /**
         * Level3: Returns true if periods are the same
         */
        public static bool operator ==(Period left, Period right)
        {
            return left.Start == right.Start && left.End == right.End;
        }

        public override string ToString()
        {
            return Start + " : " + End;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj.ToString() == ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
