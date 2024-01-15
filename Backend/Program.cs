using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using dotenv.net;

public class Program
{
    public static async Task Main(string[] args)
    {
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
                                    description
                                    url
                                    languages(first: 10) {
                                        edges {
                                            node {
                                                name
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }"
        };

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
        //                             url
        //                             defaultBranchRef {
        //                                 target {
        //                                     ... on Commit {
        //                                         history(first: 1) {
        //                                             edges {
        //                                                 node {
        //                                                     tree {
        //                                                         entries {
        //                                                             name
        //                                                             object {
        //                                                                 ... on Blob {
        //                                                                     text
        //                                                                 }
        //                                                             }
        //                                                         }
        //                                                     }
        //                                                 }
        //                                             }
        //                                         }
        //                                     }
        //                                 }
        //                             }
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

        var response = await graphQLClient.SendQueryAsync<dynamic>(query);

        Console.WriteLine(response.Data);
    }
}