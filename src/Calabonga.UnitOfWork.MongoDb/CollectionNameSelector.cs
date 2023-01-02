﻿using System.Text.RegularExpressions;

namespace Calabonga.UnitOfWork.MongoDb;

public class CollectionNameSelector : ICollectionNameSelector
{
    public string GetMongoCollectionName(string typeName)
    {
        var words = typeName.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
        var leadWord = Regex.Replace(words[0], @"([A-Z])([A-Z]+|[a-z0-9]+)($|[A-Z]\w*)", m => m.Groups[1].Value.ToLower() + m.Groups[2].Value.ToLower() + m.Groups[3].Value);

        var tailWords = words.Skip(1)
            .Select(word => char.ToUpper(word[0]) + word.Substring(1))
            .ToArray();

        return $"{leadWord}{string.Join(string.Empty, tailWords)}";
    }
}