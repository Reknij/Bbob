namespace Bbob.Plugin;

public delegate int sortArticlesDelegate(LinkInfo article1, LinkInfo article2);
public delegate int sortCategoriesDelegate(KeyValuePair<string, List<LinkInfo>> category1, KeyValuePair<string, List<LinkInfo>> category2);
public delegate int sortTagsDelegate(KeyValuePair<string, List<LinkInfo>> tag1, KeyValuePair<string, List<LinkInfo>> tag2);