using System.Dynamic;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using dotenv.net;
using Markdig;
using Newtonsoft.Json;

public class GithubService
{
    public async Task<IEnumerable<dynamic>> GetProjects()
    {
        // Your existing code to fetch and process the data goes here
        DotEnv.Load();
        var token = Environment.GetEnvironmentVariable("GITHUB_ACCESS_TOKEN");

        var graphQLClient = new GraphQLHttpClient("https://api.github.com/graphql", new NewtonsoftJsonSerializer());
        graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var query = new GraphQLRequest
        {
            Query = @"
            {
                viewer {
                    pinnedItems(first: 6, types: REPOSITORY) {
                        edges {
                            node {
                                ... on Repository {
                                    name
                                    url
                                    languages(first: 10) {
                                        edges {
                                            node {
                                                name
                                            }
                                        }
                                    }
                                    object(expression: ""main:README.md"") {
                                        ... on Blob {
                                            text
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }"
        };

        var response = await graphQLClient.SendQueryAsync<dynamic>(query);

        var projects = new List<ExpandoObject>();

        foreach (var node in (IEnumerable<dynamic>)response.Data.viewer.pinnedItems.edges)
        {
            dynamic project = new ExpandoObject();
            project.name = node.node.name;
            project.url = node.node.url;
            project.stack = ((IEnumerable<dynamic>)node.node.languages.edges).Select(lang => (string)lang.node.name).ToArray();

            // Added null check in case I forget to add a Readme to a project
            if (node.node.@object != null)
            {
                project.description = Markdown.ToPlainText(node.node.@object.text.ToString());
            }
            else
            {
                project.description = null;
            }

            dynamic projectWrapper = new ExpandoObject();
            projectWrapper.project = project;

            projects.Add(projectWrapper);
        }

        return projects;
    }
}