using RestSharp;

namespace CommandCentral.Test
{
    public static class TestUtils
    {
        public static RestRequest CreateRequest(string uri, Method method)
        {
            var request = new RestRequest(uri, method);
            request.AddHeader("X-Api-Key", "E28235AC-57A1-42AC-AA85-1547B755EA7E");
            request.AddHeader("X-Impersonate-Person-Id", "b2db659d-4998-40a2-8962-e6eb05326ea5");
            return request;
        }
    }
}