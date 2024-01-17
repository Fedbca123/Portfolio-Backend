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
                                    description
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

            // Check if node.node.name contains "-" and replace with " "
            project.name = node.node.name != null ? node.node.name.ToString().Replace("-", " ") : null;
            project.url = node.node.url;
            // Check if repo description is not null before splitting
            project.stack = node.node.description != null ? node.node.description.ToString().Split(", ") : new string[0];

            // Added null check in case I forget to add a Readme to a project
            if (node.node.@object != null)
            {
                //specific check to remove list of people for Our Cooking mama site, but not remove it from actual repo readme bc I didn't wanna do that and not give credit to my team who helped make the project
                project.description = Markdown.ToPlainText(node.node.@object.text.ToString().Replace("Our Cooking Mama", "").Replace("Project Manager: Geela Margo Ramos", "").Replace("API/Backend: Marc Cross and Taniya Shaffer", "").Replace("Mobile: Cristian Merino and Chrystian Orren", "").Replace("Frontend: Rachel Biesiedzinski and Omar Shalaby", "").Replace("Database: Iliya Klishin", "").Replace("\n-", ", ").Replace("\".\"", "\"Our Cooking Mama.\""));
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