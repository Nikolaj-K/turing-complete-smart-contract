
## Turing Complete Smart Contract

### Overview
This project presents a computationally universal and **programmable** decentralized application along with a decentralized **persistent program library** on the **NEO** blockchain.

So for example, it is the first smart contract on which general mathematical problems have been solved (from arithmetic all the way up to recognizing some unrestricted grammar) despite the source code not knowing anything about those tasks.

### Documentation
The dApp interprets encoded classical [Turing machines](https://en.wikipedia.org/wiki/Turing_machine), which is what a [universal Turing machines](https://en.wikipedia.org/wiki/Universal_Turing_machine) does. The following documentation video elaborates on this in 7 parts:

```
00:01   Overview
06:55   Use of the dApp on the NEO testnet
11:38   Historical context
18:24   Turing completeness explained
54:40   In depth code review
```

[![DocumentationVideo](https://i.imgur.com/4PFvhpH.png)](https://youtu.be/CAUo5aNmvz8 "Documentation Video: A Turing complete smart contract on the NEO blockchain")

The last 30 minutes cover the code in detail. The arguments for the main functions are two strings and an array of integers. The first string denotes an auxiliary name for a machine to be uploaded on the blockchain. For programmer convenience, a Turing machine can be encoded as a string and is provided as the second argument. For efficiency sake, beyond testnet use, the "Assemble" step should be performed offline before the machine is deployed. Once there is a machine with the chosen name on the blockchain, then when invoking this name again, the second argument is used as the input string for the machine and is executed. The third argument is an array of integers that can optionally be used to modify default specifications such as the size of used character alphabet and maximal runtime measures as actions on the Turing machine tape.

The programming language is explained in the forth part of the video in detail.

The config array in

`int Main(string program_name, string code, params object[] config)`

holds, in this order, used alphabet size (2), accept state index (1), max executed steps (len^2), tape padding (len), head position relative to tape input of length `len` (0), start state index (0). The default values are listed in brackets.

#### Testnet script hash

`0xef13fd4885ebffabfaac6e071ea87922376be7c0`

#### Simplest example

As an example, the machine encoded as sequence of 3 strings of length three given by

```haskell
"010, 010, 011"
```

detects whether an input string from the 3-letter-alphabet `{'x', '0', '1'}` contains the character '1'. It does encode the short program

```haskell
foreach (char : tape)
  if (current_state == start_state):
    if char is 'x':  overwrite with '0',  move right on tape,  go to start state,
    if char is '0':  overwrite with '0',  move right on tape,  go to start state,
    if char is '1':  overwrite with '0',  move right on tape,  go to accept state,  return "Accepted!"
    
return "Rejected!"
```

In such a way, a program with k states and possible m characters is of length 3 * k * m.

One short program is integer addition, stored under the program name `"add"` and addition of 3+3 is encoded by sequences of 1's seperated by a zero, i.e. `"1110111"`

Finite state machines are particularly straight forward to program, as in that case data in the memory is never visited a second time. The video above contains an example of a Turing machine that detects whether or not an input string is of length 2^n for some n. It also demonstrates basic arithmetic with Turing machines.

This encoding is capable but of course extremely low level, from a programming language perspective. Moreover, the Turing model of computation, with its restricted steps size on the memory tape, doesn't make for an efficient computer. 

### License
This project is authored by Nikolaj Kuntner and is MIT licensed.
