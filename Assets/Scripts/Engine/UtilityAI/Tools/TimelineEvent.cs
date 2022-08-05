using System;

namespace UtilityAI
{
    public struct TimelineEvent<T>: IEquatable<TimelineEvent<T> >  where T : IEquatable<T>
    {
        public T Reference;
        public float StartTime;
        public float EndTime;

        public float Duration => EndTime - StartTime;

        public TimelineEvent(T reference, float startTime)
        {
            Reference = reference;
            StartTime = startTime;
            EndTime = startTime;
        }

        public bool Equals (TimelineEvent<T> other)
        {
            return this.Reference.Equals(other.Reference) && this.StartTime == other.StartTime && this.EndTime == other.EndTime;
        }

        public override string ToString () => $"[{Duration.ToString("F1").PadLeft(6,' ')}] {Reference}";
    }
}