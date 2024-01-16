using System.Dynamic;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using dotenv.net;
using Markdig;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Program
{
    public static async Task Main(string[] args)
    {
        await GetGithubInfo();
    }

    public static async Task GetGithubInfo()
    {
        DotEnv.Load();
        var token = Environment.GetEnvironmentVariable("GITHUB_ACCESS_TOKEN");

        var graphQLClient = new GraphQLHttpClient("https://api.github.com/graphql", new NewtonsoftJsonSerializer());
        graphQLClient.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // var query = new GraphQLRequest
        // {
        //     Query = @"
        //     {
        //         viewer {
        //             pinnedItems(first: 6, types: REPOSITORY) {
        //                 edges {
        //                     node {
        //                         ... on Repository {
        //                             name
        //                             description
        //                             url
        //                             languages(first: 10) {
        //                                 edges {
        //                                     node {
        //                                         name
        //                                     }
        //                                 }
        //                             }
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //     }"
        // };

        // var response = await graphQLClient.SendQueryAsync<dynamic>(query);

        // // string markdown = /* your markdown content */;
        // // string plainText = Markdig.Markdown.ToPlainText(markdown);

        // // Console.WriteLine(response.Data);

        // string json = JsonConvert.SerializeObject(response.Data, Formatting.Indented);

        // Console.WriteLine(json);

        // Modify your GraphQL query to fetch the README file
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

            // Add a null check before accessing the text property
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

        string json = JsonConvert.SerializeObject(projects, Formatting.Indented);

        Console.WriteLine(json);
    }
}