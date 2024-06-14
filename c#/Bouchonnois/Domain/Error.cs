namespace Bouchonnois.Domain
{
    public record Error
    {
        public string Message { get; }

        private Error(string message) => Message = message;

        public static Error AnError(string message) => new(message);
    }
}