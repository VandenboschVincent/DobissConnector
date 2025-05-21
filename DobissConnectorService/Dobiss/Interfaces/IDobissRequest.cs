namespace DobissConnectorService.Dobiss.Interfaces
{
    /// <summary>
    /// Defines a request that can be sent via DobissClient.
    /// </summary>
    /// <typeparam name="O">The type of the response object.</typeparam>
    public interface IDobissRequest<requestType>
    {
        /// <summary>
        /// Gets the raw byte array to send for this request.
        /// </summary>
        /// <returns>Request bytes.</returns>
        byte[] GetRequestBytes();

        /// <summary>
        /// Indicates the maximum number of output lines expected from Dobiss for this request.
        /// Used for performance (fewer lines → fewer actual requests).
        /// If zero, the client’s default will be used.</summary>
        /// <returns>Maximum output lines.</returns>
        int GetMaxOutputLines();

        /// <summary>
        /// Executes the request and returns the deserialized result.</summary>
        /// <returns>An object of type O representing the response.</returns>
        Task<requestType> Execute(CancellationToken cancellationToken);
    }
}
