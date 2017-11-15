using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;


namespace Neo.SmartContract
{
    public class MachineLibraryAndInterpreter : Framework.SmartContract
    {
        public static int Main(string program_name, string code, params object[] config)
        {
            // Depending on whether there is a Turing machine code under the 
            // key 'program_name' on the blockchain or not, either put 'code'
            // in storage or execute the loaded machine with 'code' as input 

            Runtime.Log(program_name);

            byte[] byte_code = Storage.Get(Storage.CurrentContext, program_name);

            if (byte_code == null)
                return Deploy(program_name, code);
            else
                return Run(byte_code, code, config);
        }


        public static int Deploy(string program_name, string code)
        {
            // Put machine code in persistent programming library

            Storage.Put(Storage.CurrentContext, program_name, code);

            Runtime.Log("Program code successfully deployed!");
            return 0;
        }


        public static int Run(byte[] byte_code, string code, params object[] config)
        {
            // Execute the program 'byte_code' with 'code' as input.

            int[] machine = Assemble(byte_code.AsString());
            int[] tape_sequence = Assemble(code);
            // Note: The Assemble method calls above decode input and enable human 
            // readable programming. For efficiency of a dApp on mainnet, this 
            // convenience method should be performed by the coder offline.

            //----------------------------------------------------------- setup

            BigInteger len = tape_sequence.Length;
            int row = 0;

            // Set default values
            int alphabet = 2;
            int accept_state = 1;
            BigInteger runtime = len * len;
            int padding = (int)len;
            int head = 0;
            int state = 0;

            // Overwrite defaults with optional configuration input
            if (config.Length > 0) alphabet = (int)config[0];
            if (config.Length > 1) accept_state = (int)config[1];
            if (config.Length > 2) runtime = (int)config[2];
            if (config.Length > 3) padding = (int)config[3];
            if (config.Length > 4) head = padding + (int)config[4];
            if (config.Length > 5) state = (int)config[5];
            // Note: 'accept_states' could be made a byte array of several such states


            // Put the input data on the center of a tape with padding left and right
            BigInteger tape_length = len + 2 * padding;

            int[] tape = new int[(int)tape_length];

            for (int i = 0; i < len; i++)
                tape[padding + i] = tape_sequence[i];

            //----------------------------------------------------------- blockchain access

            // A Turing machine could be given access to NEO system functions in the following way.
            // For demonstration, here we enable querying the current timestamp.

            int query_instruction = 35; // corresponds to 'Z'

            if (machine[row] == query_instruction)
            {
                Header header = Blockchain.GetHeader(Blockchain.GetHeight());

                Runtime.Notify("NEO blockchain timestamp", header.Timestamp);
                return 5;
            }

            //----------------------------------------------------------- main loop 
            
            for (int n = 0; n < runtime; n++)
            {
                if (head >= tape_length)
                {
                    Runtime.Notify("Exception: Machine ran out of tape", head);
                    return 3;
                }

                // Obtain write symbol index
                row = (state * alphabet + tape[head]) * 3;

                if (row + 3 > machine.Length)
                {
                    Runtime.Notify("Error: Incompatible data", row);
                    return 4;
                }

                // write the write symbol to the tape
                tape[head] = machine[row];

                // Move the writer head
                head += HeadDelta(machine, row);
                
                // Change states according to the transition function
                state = machine[row + 2];

                // for several accept states, do foreach (int ac in accept_states)
                if (state == accept_state)
                {
                    Runtime.Notify("Accepted!", accept_state, EvaluateTape(tape));
                    return 1;
                }
            }

            Runtime.Notify("Rejected!", state);
            return 2;
        }

        public static int[] Assemble(string hex_code)
        {
            // Enable user to input data as strings with character 0 to 9 and A to Z.
            // Note: Such an encoding can be performed offline. For this change the 
            // string argument type of Main to byte[].

            int n;
            int i = 0;

            int[] assembly = new int[hex_code.Length];

            foreach (char chr in hex_code)
            {
                n = chr - '0';
                if (0 <= n && n <= 9)
                    assembly[i++] = n;

                n = chr - 'A';
                if (0 <= n && n <= 25)
                    assembly[i++] = 10 + n;
            }

            return assembly;
        }


        public static int EvaluateTape(int[] tape)
        {
            // Return the quantity of '1' on the final tape

            int count = 0;

            foreach (int symbol in tape)
            {
                if (symbol == 1)
                    count++;
            }

            return count;
        }


        public static int HeadDelta(int[] machine, int row)
        {
            // In the chosen encoding, 2 represents a step to the left.

            int delta = machine[row + 1];

            if (delta == 2)
                return -1;

            return delta;

            // Note: The machine could be adapted to include bigger jumps on the tape.
        }
    }
}
