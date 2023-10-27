// 1- Bibliotecas
// dependencia para o JsonConvert
using Models;
using Newtonsoft.Json;
using RestSharp;

// 2- NameSpace
namespace Pet;

// 3- Classe
public class PetTest
{
    // 3.1- Atributos
    private const string BASE_URL = "https://petstore.swagger.io/v2/";
    // public String token;  // todo - arquivo UserTest.cs para usar em toda a classe UserTest (ou usa como Environment)

    // 3.2- Funções e Métodos

    // Função de leitura de dados a partir de um arquivo csv
    public static IEnumerable<TestCaseData> getTestData()
    {
        String caminhoMassa = @"C:\Iterasys\FTS139\PetStore139\fixtures\pets.csv";

        using var reader = new StreamReader(caminhoMassa);

        // Pula a primeira linha com os cabeçalhos
        reader.ReadLine();

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(",");

            yield return new TestCaseData(int.Parse(values[0]), int.Parse(values[1]), values[2], values[3], values[4], values[5], values[6], values[7]);
        }

    }



    [Test, Order(1)]
    public void PostPetTest()
    {
        // Configura
        // instancia o objeto do tipo RestClient com o endereço da API
        var client = new RestClient(BASE_URL);

        // instancia o objeto do tipo RestRequest com o complemento de endereço da API como "pet" e configura o metodo como POST
        var request = new RestRequest("pet", Method.Post);

        // armqzena o conteúdo do arquivo pet.json na memória
        String jsonBody = File.ReadAllText(@"C:\Iterasys\FTS139\PetStore139\fixtures\pet1.json");

        // adiciona na requisição o conteúdo do arquivo pet1.json
        request.AddBody(jsonBody);

        // Executa
        // executa a requisição conforme configurado e guarda o json retornado no objeto response
        var response = client.Execute(request);

        // Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        // Exibe o responseBody no console
        Console.WriteLine(responseBody);

        // int convert texto em número
        // valida resultado obtido com resultado esperado
        Assert.That((int)response.StatusCode, Is.EqualTo(200)); // Comunicação OK

        // Valida  petId
        int petId = responseBody.id;              // lendo do json
        Assert.That(petId, Is.EqualTo(532321));

        // cria name e pega o conteúdo da responseBody e converte para texto
        string name = responseBody.name.ToString();
        // valida o nome do pet
        Assert.That(name, Is.EqualTo("Tinker Bell"));
        // OU
        // Assert.That(responseBody.name.ToString(), Is.EqualTo("Tinker Bell"));

        // valida o status do pet
        String status = responseBody.status;
        Assert.That(status, Is.EqualTo("available"));

        // Armazenar os dados obtidos para usar nos próximos testes
        Environment.SetEnvironmentVariable("petId", petId.ToString());
    }

    [Test, Order(2)]
    public void GetPetTest()
    {
        // Configura
        int petId = 532321;               // campo de pesquisa
        String petName = "Tinker Bell";  // resultado esperado
        String categoryName = "turtle";
        String tagsName = "chipado";

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"pet/{petId}", Method.Get);

        // Executa
        var response = client.Execute(request);

        // Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.id, Is.EqualTo(petId));
        Assert.That((String)responseBody.name, Is.EqualTo(petName));

        Assert.That((String)responseBody.category.name, Is.EqualTo(categoryName));
        Assert.That((String)responseBody.tags[0].name, Is.EqualTo(tagsName));

    }

    [Test, Order(3)]
    public void PutPetTest()
    {
        // Configura
        // Os dados de entrada vão formar o body da alteração, usando uma classe de modelo dinâmica (como se fosse um formulário)
        PetModel petModel = new PetModel();
        petModel.id = 532321;
        petModel.category = new Category(9, "turtle");
        petModel.name = "Tinker Bell";
        petModel.photoUrls = new string[] { "" }; // vazio
        petModel.tags = new Tag[] { new Tag(1, "femea"), new Tag(2, "vacinado"), new Tag(3, "chipado") };
        petModel.status = "sold";

        // transforma o objeto acima em um arquivo json
        String jsonBody = JsonConvert.SerializeObject(petModel, Formatting.Indented);
        Console.WriteLine(jsonBody); // escreve o arquivo no console

        var client = new RestClient(BASE_URL);             // endereço da requisição
        var request = new RestRequest("pet", Method.Put);  // metodo da requisição
        request.AddBody(jsonBody);                         // corpo da requisição

        // Executa
        var response = client.Execute(request);

        // Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.id, Is.EqualTo(petModel.id));
        Assert.That((String)responseBody.tags[0].name, Is.EqualTo(petModel.tags[0].name));
        Assert.That((String)responseBody.tags[1].name, Is.EqualTo(petModel.tags[1].name));
        Assert.That((String)responseBody.status, Is.EqualTo(petModel.status));
    }

    [Test, Order(4)]
    public void DeletePetTest()
    {
        // Configura
        String petId = Environment.GetEnvironmentVariable("petId");

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"pet/{petId}", Method.Delete);

        // Executa
        var response = client.Execute(request);


        // Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.code, Is.EqualTo(200));
        Assert.That((String)responseBody.message, Is.EqualTo(petId));
    }


    // Ler massa de teste de um arquivo csv - Data Driven Test

    [TestCaseSource("getTestData", new object[] { }), Order(5)]
    public void PostPetDDTest(
                                int petId,
                                int categoryId,
                                String categoryName,
                                String petName,
                                String photoUrls,
                                String tagsIds,
                                String tagsName,
                                String status
                             )
    {
        // Configura

        PetModel petModel = new PetModel();
        petModel.id = petId;
        petModel.category = new Category(categoryId, categoryName);
        petModel.name = petName;
        petModel.photoUrls = new string[] { photoUrls };

        // Código para gerar as multiplas tags que o pet pode ter
        String[] tagsIdsList = tagsIds.Split(";");    // Ler
        String[] tagsNameList = tagsName.Split(";"); // Ler
        List<Tag> tagList = new List<Tag>();         // Gravar depois do for

        for (int i = 0; i < tagsIdsList.Length; i++)
        {
            int tagId = int.Parse(tagsIdsList[i]);
            String tagName = tagsIdsList[i];

            Tag tag = new Tag(tagId, tagName);
            tagList.Add(tag);
        }

        petModel.tags = tagList.ToArray();
        petModel.status = status;

        // A estrutura de dados está pronta, agora vamos serializar
        String jsonBody = JsonConvert.SerializeObject(petModel, Formatting.Indented);
        Console.WriteLine(jsonBody);  // mostrar o arquivo montado no console

        // instancia o objeto do tipo RestClient com o endereço da API
        var client = new RestClient(BASE_URL);

        // instancia o objeto do tipo RestRequest com o complemento de endereço da API como "pet" e configura o metodo como POST
        var request = new RestRequest("pet", Method.Post);

        // adiciona na requisição o conteúdo do arquivo pet1.json
        request.AddBody(jsonBody);

        // Executa
        // executa a requisição conforme configurado e guarda o json retornado no objeto response
        var response = client.Execute(request);

        // Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        // Exibe o responseBody no console
        Console.WriteLine(responseBody);

        // int convert texto em número
        // valida resultado obtido com resultado esperado
        Assert.That((int)response.StatusCode, Is.EqualTo(200)); // Comunicação OK

        // Valida o petId na resposta
        Assert.That((int)responseBody.id, Is.EqualTo(petId));

        // valida o nome do pet na resposta
        Assert.That((String)responseBody.name, Is.EqualTo(petName));

        // valida o status do pet na resposta
        Assert.That((String)responseBody.status, Is.EqualTo(status));
    }

    // todo - em outro arquivo
    [Test, Order(6)]
    public void GetUserLoginTest()
    {
        // Configura
        String username = "tutu";
        String password = "654321";

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"user/login?username={username}&password={password}", Method.Get);

        // Executa
        var response = client.Execute(request);

        // Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content); // resposta é sempre deserializada
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.code, Is.EqualTo(200));

        // Extrair o token da resposta - usando Environment
        String message = responseBody.message;
        String token = message.Substring(message.LastIndexOf(":") + 1); // vai pegar do ":" mais uma posição depois
        Console.WriteLine($"Token = {token}");

        Environment.SetEnvironmentVariable("token", token);            // guarda numa variável de ambiente e pode ser usada em qualquer outro método
    }
}