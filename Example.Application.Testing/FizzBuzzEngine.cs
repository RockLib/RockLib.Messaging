using RockLib.Messaging;
using System;
using System.Threading.Tasks;

namespace Example
{
    public class FizzBuzzEngine
    {
        public FizzBuzzEngine(ISender sender) =>
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));

        public ISender Sender { get; }

        public async Task SendFizzBuzzMessage(long value)
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value), "Must be greater than zero.");

            if (value % 15 == 0)
                await Sender.SendAsync("fizz-buzz");
            else if (value % 3 == 0)
                await Sender.SendAsync("fizz");
            else if (value % 5 == 0)
                await Sender.SendAsync("buzz");
            else
                await Sender.SendAsync(value.ToString());
        }
    }
}
