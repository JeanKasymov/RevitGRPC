using Grpc.Core;
using RevitOutOfContext_gRPC_ProtosF;

namespace GrpcServerConsole.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly Greeter.GreeterClient _client;

        public GreeterService(Greeter.GreeterClient client)
        {
            _client = client;
        }
        public override async Task ServerDataStream(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<CommandReply> responseStream, ServerCallContext context)
        {
            var readTask = Task.Run(async ()  =>
            {
                var famNamesAndCats = new List<(string, string)>();
                await foreach (var request in requestStream.ReadAllAsync())
                {
                    famNamesAndCats.Add((request.ProcesId, request.Text));
                }
                DBService.RecordFamiliesDB(famNamesAndCats);
            });
            await readTask;
        }
        public override Task<CommandReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            CommandReply reply = new CommandReply();

            if (!string.IsNullOrEmpty(request.Name))
            {
                if (request.Name == "Command")
                {
                    Commands.AddCommand(request.Text);
                    reply.Command = "Added";
                }
                else if (request.Name == "DllPathRequest")
                {
                    var command = Commands.GetCommand();
                    reply.Command = command;
                }
                else if (request.Name == "DllResult")
                {
                    Commands.AddCommandResult(request.ProcesId, request.Text);
                }
                else if (request.Name == "GetDllResult")
                {
                    reply.Command = Commands.GetCommandResult(request.ProcesId);
                }
                else if (request.Name == "CommandWithResult")
                {
                    Commands.AddCommand(request.Text);
                    while (true)
                    {
                        if (Commands.CommandsResult.Count > 0)
                        {
                            reply.Command = "Added";
                            Commands.CommandsResult.Clear();
                            break;
                        }
                    }
                }
                //else if (request.Name == "FamilyDBRequest")
                //{
                //    DBService.RecordFamilyDB(request.ProcesId, request.Text);
                //    reply.Command = "ReplyDone";
                //}
            }
            return Task.FromResult(reply);
        }
    }
}