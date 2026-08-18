# labIA-Docker — DocChat

A didactic terminal chat that answers questions grounded in your own documents (RAG), built with
[Microsoft.Agents.AI](https://learn.microsoft.com/agent-framework/) and
[Microsoft.Extensions.AI](https://learn.microsoft.com/dotnet/ai/), running entirely against local
models served from Docker containers — no cloud LLM API required.

## How it works

- **Agentic RAG**: the AI agent itself decides, via tool-calling, when to search the knowledge
  base. There's no hardcoded "always retrieve before answering" step — the `SearchDocuments` tool
  is just offered to the agent, and the model calls it when it judges the question needs it.
- **Fresh index on every run**: on startup, the app reads every `.md` file in
  `DocChat/KnowledgeBase/`, splits them into overlapping chunks (using Semantic Kernel's
  `TextChunker`), embeds them, and re-indexes them into Qdrant from scratch. Edit the knowledge
  base, restart the app, and the index reflects the change.
- **Short-term memory**: the chat history for the current terminal session is kept in memory as a
  sliding window of the last N messages (`Session:HistoryWindowSize` in `appsettings.json`). It's
  not persisted anywhere — restart the app and the conversation starts over.

Three pieces of AI infrastructure run as Docker containers, kept intentionally separate so each
one's role is obvious:

| Container    | Role                          | Image                                                  |
|--------------|-------------------------------|---------------------------------------------------------|
| `ollama`     | Chat LLM                      | `ollama/ollama`                                        |
| `embeddings` | Embedding model                | `ghcr.io/huggingface/text-embeddings-inference`         |
| `qdrant`     | Vector database                | `qdrant/qdrant`                                         |

## Prerequisites

- .NET 10 SDK
- Docker Desktop (or another Docker Engine + Compose v2)

## Running it

1. Copy the environment file and adjust it if you want a different embedding model:

   ```
   cp .env.example .env
   ```

2. Start the containers:

   ```
   docker compose up -d
   ```

   This starts Qdrant, Ollama, and the embeddings service. The embeddings container downloads its
   model from the Hugging Face Hub the first time it starts, which can take a minute.

3. Run the chat app:

   ```
   dotnet run --project DocChat
   ```

   On first run, the app pulls the configured Ollama chat model (`qwen3:1.7b` by default) if it's
   not already present in the container — this can take a while depending on your connection.
   After that, it indexes `DocChat/KnowledgeBase/*.md` into Qdrant and drops you into a prompt.

4. Ask questions grounded in the sample knowledge base (basic high-school physics: kinematics,
   Newton's laws, work/energy/power), or replace the `.md` files with your own documents. Type
   `exit` to quit.

## Configuration

`qwen3:1.7b` is the default chat model because Ollama here runs on CPU only (no GPU passthrough
configured) — it favors a responsive terminal chat over raw answer quality. If your machine has a
usable GPU, or you don't mind multi-minute responses, set `Ollama:ChatModel` to a larger
tool-calling model such as `qwen3:8b` for better answers.

All settings live in `DocChat/appsettings.json` and can be overridden with environment variables
(e.g. `Ollama__ChatModel=llama3.2` `dotnet run --project DocChat`):

| Section                         | Purpose                                              |
|----------------------------------|-------------------------------------------------------|
| `Ollama:BaseUrl`, `Ollama:ChatModel` | Chat LLM connection and model name               |
| `Embeddings:BaseUrl`, `Embeddings:Model` | Embedding service endpoint and model         |
| `Qdrant:Host`, `Qdrant:Port`, `Qdrant:CollectionName` | Vector DB connection            |
| `Chunking:*`                     | Chunk size / overlap for splitting documents          |
| `Session:HistoryWindowSize`      | How many recent chat messages are kept in memory       |
| `Documents:Path`                 | Folder (relative to the app) to load `.md` files from  |

If you change `Embeddings:Model` to a model with a different output size than 384 dimensions,
also update the `EmbeddingSettings.Dimensions` constant in `DocChat/Search/DocumentChunk.cs` —
the vector store attribute that declares the embedding size needs a compile-time constant.

## Project layout

```
DocChat/
  Program.cs                  # bootstrap + terminal chat loop
  Configuration/               # strongly-typed appsettings.json bindings
  Ingestion/                   # loads .md files, chunks them, upserts into Qdrant
  Search/                      # DocumentChunk vector record + the SearchDocuments agent tool
  Agent/                       # builds the AIAgent with its instructions and tools
  Chat/                        # sliding-window session memory
  Infrastructure/              # Ollama, embeddings, and Qdrant client/store setup
  KnowledgeBase/                # sample .md documents indexed on startup
```
