# Building an Evolutionary Fuzzer

> *"What happens if I teach a Python script to 'evolve' malicious inputs until it breaks a C# API?"*

That was the question I wanted to answer with this project. I've been reading about **Evolutionary Fuzzing** and Microsoft's **Variant Hunting** methodologies, so rather than just using tools like Burp Suite, I decided to build my own fuzzer from scratch to understand the logic behind it.

### The Experiment
I created a lab environment 
1.  A C# .NET Core Web API that I intentionally engineered with a "Logic Bomb" (a hidden crash that only triggers if the input length is exactly 42 characters).
2.  A Python script that doesn't just guess randomly—it uses **Genetic Algorithms**. It takes "parents" (inputs), mixes them (crossover), mutates them, and learns from the server's response.

![Crash Proof]<img width="1887" height="608" alt="image" src="https://github.com/user-attachments/assets/4149df59-fde5-4964-9801-3a5a33483e39" />

*The moment the fuzzer figured out the logic bomb. Left: Python finding the crash. Right: The C# server exploding.*

---

### Why Evolutionary?
Standard fuzzers are "dumb"—they just throw random trash at the server. 
I wanted to implement an **OODA Loop** (Observe, Orient, Decide, Act).

* **Observe:** The fuzzer checks the API response. Did the response length change? That means we hit a new code path!
* **Orient:** If we found a new path, add that input to the "Gene Pool."
* **Decide:** Pick two successful parents and breed them (or mutate them).
* **Act:** Fire the new child payload.

I also implemented a **"Dictionary Mutation"** strategy. Pure evolution takes a long time, so I taught the fuzzer to occasionally "cheat" by injecting known bad strings (like `NULL` bytes or long buffer overflows). This turned out to be the key to finding the bug quickly.

---

### 💥 The Crash
The fuzzer successfully identified the logic bomb (`data.Payload.Length == 42`) in Generation 0 by using a dictionary replacement strategy.

Here is the actual Proof of Concept (PoC) it generated automatically:

![PoC JSON]<img width="902" height="93" alt="image" src="https://github.com/user-attachments/assets/dc1cefd5-e95b-4815-9290-f6103ab8a955" />


It flagged the crash as "Unique" by hashing the stack trace, so I didn't get spammed with 1,000 logs for the same bug.

---

### How I Built It
**The Target (C# .NET 8.0)**
I simulated a "Critical Memory Failure" using an unhandled exception.
```csharp

The Logic Bomb in Program.cs 
if (data.Payload.Length == 42)
{
    throw new InvalidOperationException("CRITICAL MEMORY FAILURE: Buffer Edge Case Hit!");
}
