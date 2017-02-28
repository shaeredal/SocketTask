using System;

namespace SocketLib.Models
{
    public class MethodKeyModel : IEquatable<MethodKeyModel>
    {
        public string Name { get; set; }
        public int ParametersCount { get; set; }

        public bool Equals(MethodKeyModel other)
        {
            return (object)other != null && 
                (string.Equals(Name, other.Name) && 
                ParametersCount == other.ParametersCount);
        }

        public static bool operator ==(MethodKeyModel a, MethodKeyModel b)
        {
            return (object)a != null && a.Equals(b);
        }

        public static bool operator !=(MethodKeyModel a, MethodKeyModel b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj.GetType() == this.GetType() && Equals((MethodKeyModel)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name?.GetHashCode() ?? 0) * 397) ^ ParametersCount;
            }
        }
    }
}
