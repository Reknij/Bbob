namespace Bbob.Plugin;

public delegate int sortArticlesDelegate(dynamic article1, dynamic article2);
public delegate int sortCategoriesDelegate(KeyValuePair<string, List<dynamic>> category1, KeyValuePair<string, List<dynamic>> category2);
public delegate int sortTagsDelegate(KeyValuePair<string, List<dynamic>> tag1, KeyValuePair<string, List<dynamic>> tag2);