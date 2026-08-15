// Jakar.Extensions :: Jakar.Extensions
// 07/11/2025  15:58

namespace Jakar.Shapes;


public static class Shapes
{
    extension<TValue>( TValue[]? self )
    {
        public TValue[]? Create( RefSelect<TValue> func )
        {
            ReadOnlySpan<TValue> span = self;
            if ( span.IsEmpty ) { return null; }

            TValue[] buffer = GC.AllocateUninitializedArray<TValue>(span.Length);
            int      index  = 0;

            foreach ( ref readonly TValue value in span ) { buffer[index++] = func(in value); }

            return buffer;
        }
        public TOutput[]? Create<TOutput>( RefSelect<TValue, TOutput> func )
        {
            ReadOnlySpan<TValue> span = self;
            if ( span.IsEmpty ) { return null; }

            TOutput[] buffer = GC.AllocateUninitializedArray<TOutput>(span.Length);
            int       index  = 0;

            foreach ( ref readonly TValue value in span ) { buffer[index++] = func(in value); }

            return buffer;
        }
    }
}
