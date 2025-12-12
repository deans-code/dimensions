# Dimensions

## :movie_camera: Background

As per [Google's definition](https://cloud.google.com/discover/what-is-semantic-search):

> Semantic search is a data searching technique that focuses on understanding the contextual meaning and intent behind a user’s search query, rather than only matching keywords. 

This application was designed as a simple tool for testing the capabilities of embeddings when used as part of semantic search use cases.

The software follows these steps to generate a semantic search database:
- Using a system prompt and list of topics, a local LLM generates a set of archival data documents.
- A chunking strategy is applied to break the archival data into more focussed documents.
- Contextualisation is applied to each chunk in order to retain the wider document's context.
- A local text embedding model generates embeddings for each chunk, the embeddings are normalised.
- The normalised embeddings are stored within a vector database.

Once the database exists, the user can submit search queries which are converted into embeddings and then compared against the data within the vector database.

This concept can form part of a Retrieval Augmented Generation (RAG) solution, where the resulting relevant documents can be passed into a LLM to support the generation of a response to the user's original query.

## :white_check_mark: Scope

- [x] Create simple console application.
- [x] Integrate local language model.
- [x] Integrate local vector database.
- [x] Provide search function.
- [x] Apply normalisation to embeddings.
- [x] Implement basic chunking, use markdown format for input.
- [x] Implement contextualisation, use alternative local LLM.
- [ ] Test with alternative embeddings model.
- [ ] Test with alternative chat completion model.

## :telescope: Future Gazing

- [ ] Explore capabilities of the Qdrant vector database, understand how search queries can be adjusted to affect results. 
- [ ] Consider adding a lexical search option for comparing results.

## :beetle: Known defects

No known defects.

## :crystal_ball: Use of AI

[GitHub Copilot](https://github.com/features/copilot) was used to assist in the development of this software.

## :rocket: Getting Started

### :computer: System Requirements

#### Software

![Windows](https://img.shields.io/badge/Windows-11-blueviolet "Windows")
![.NET](https://img.shields.io/badge/.NET-latest_9.x.x-blueviolet ".NET")
![LM Studio](https://img.shields.io/badge/LM_Studio-latest-blueviolet "LM Studio")
![VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-latest-blueviolet "VS Code Insiders")
![Docker Desktop](https://img.shields.io/badge/Docker_Desktop-latest-blueviolet "Docker Desktop")
![Postman](https://img.shields.io/badge/Postman-latest-blueviolet "Postman")

> [!NOTE]
> Other operating systems and versions will work, where versions are specified treat as minimums.

#### Hardware

A system capable of running LM Studio is required.

Details of my personal system are below.

![APU](https://img.shields.io/badge/APU-AMD_Ryzen_AI_Max_395+-yellow "APU")

> [!NOTE]
> The hardware in use on my PC includes an Accelerated Processor Unit (APU) which combines CPU and GPU on a single chip. Recommendations for alternative hardware can be found [here](https://lmstudio.ai/docs/app/system-requirements), performance will depend upon the models you choose to run (and other operational factors).

### :floppy_disk: System Configuration

#### LM Studio

Configure LM Studio as per the [documentation](https://lmstudio.ai/docs/app/basics).

Download:
- an appropriate text embedding model,
- an appropriate LLM for chat completion.

> [!NOTE] 
> You can use [community leaderboards](https://huggingface.co/spaces/OpenEvals/find-a-leaderboard) to help select appropriate models.

Use the Developer tab to run your chosen models using the [API server](https://lmstudio.ai/docs/app/api).

You can use [Postman](https://www.postman.com/) to test access to the endpoints.

If testing your text embeddings model using the default options, you can test the local server by configuring a `POST` request with the following parameters:

URL:
```
http://127.0.0.1:1234/v1/embeddings
```

Headers:
```
 Content-Type: application/json
```

Body (raw):
```
{
    "input": "Hello world!"
}
```

You should see a response which includes the embedding values:

```
{
    "object": "list",
    "data": [
        {
            "object": "embedding",
            "embedding": [
                0.03805531933903694,
                0.032784245908260345,                
                ...
                -0.006903552915900946,
                -0.02046305313706398
            ],
            "index": 0
        }
    ],
    "model": "text-embedding-embeddinggemma-300m",
    "usage": {
        "prompt_tokens": 0,
        "total_tokens": 0
    }
}
```
#### Application

The `appsettings.json` file manages the application settings.

Review the file and ensure that the settings are appropriate for your local environment.

E.g. update the models names as required:

```json
{
  "EmbeddingApi": {    
    "Model": "text-embedding-embeddinggemma-300m"
  },
  "ChatCompletionApi": {    
    "Model": "openai/gpt-oss-120b"
  },  
}
```

The data under test can be configured. The software is designed to use your chosen LLM to create archival data to index.

> [!NOTE]
> The quality of your archival data will depend on the model you choose. Consider trying multiple models to generate archival data.

The system is configured to use the `game-historian.md` system prompt when generating archival data. You may choose to write an alternative system prompt for generating archival data. If you do, update the configuration with the new prompt location:

```json
{    
    "SystemPromptPath": "path/to/system-prompt.md",    
}
```

The `ContextSectionChunkTitles` setting specifies which sections from your archival data markdown capture a useful summary of the document's content, these sections will be added to all document chunks to maintain context.

The `ArchivalTopics` setting specifies topics for the generation of archival data. These topics will be passed into your chosen LLM along with the system prompt to generate archival data.

You may choose to adjust either of these settings if you author your own system prompt or change the topics to be searched.

Related settings:

```json
{
   "Contextualisation": {
    "ContextSectionChunkTitles": [
      "A title defined by your system prompt's template"      
    ]
  },
  "ArchivalTopics": [
    "A related topic which can be processed by the system prompt"    
  ]
}
```
### :wrench: Development Setup

Clone the repository.

Open in Visual Studio code.

Build the projects.

## :zap: Features

- Creation of archival data using a local LLM.
- Generation of normalised embeddings of archival data using local text embedding model.
- Storage of embeddings in local vector database.
- Submission of search queries, comparison against stored embeddings.

## :paperclip: Usage

Start the [Qdrant](https://qdrant.tech/) vector database Docker container, the configuration for which is located in the `docker` directory.

Start LM Studio and ensure that both your text embedding model and LLM are running:

![LM Studio](./docs/lm-studio.png)

> [!NOTE]
> If you are unable to run both models simultaneously due to lack of resources, consider running the LLM only while generating archival data. You can then eject the model and load your text embedding model for indexing and searching.

Hit F5 in VS Code to begin debugging.

The application is configured to load within the integrated terminal, you should be presented with multiple options:

![Terminal](./docs/terminal.png)

Create your archival data files, if they do not yet exist:

```bash
1. Create archival data files
```

> [!NOTE]
> This operation can take a long time to complete. Consider adjusting the system prompt and related archival data settings to simplify the operation. Note that simplifying or reducing the archival data will affect the semantic meaning and search capabilities.

When your archival data files have been created, create the vector database:

```bash
1. Create vector database from data files
```

You can view the content of your vector database using the following URL: 
http://localhost:6333/dashboard

![Qdrant](./docs/qdrant.png)

Once you have data within your vector database, you can perform a search:

```bash
4. Enter search text
```

You will then see results which display a relevancy score:

![Search](./docs/search.png)

## :wave: Contributing

This repository was created primarily for my own exploration of the technologies involved.

## :gift: License

I have selected an appropriate license using [this tool](https://choosealicense.com/).

This software is licensed under the [MIT](LICENSE) license.

## :book: Further reading

More detailed information can be found in the documentation:
* [Resources](docs/resources.md)