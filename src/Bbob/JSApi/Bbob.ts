export interface LinkInfo {
    title: string,
    date: string,
    categories?: string[],
    tags?: string[]
    address: string
}
export interface Article {
    title: string,
    date: string,
    categories?: string[],
    tags?: string[],
    contentParsed?: string
    toc?: string
}
export interface ArticleProcessCallBack {
    (article: Article): void
}
export interface LinkInfoArrayProcessCallBack {
    (linkArray: LinkInfo[]): void
}
export interface BbobMeta{
    blogName: string,
    author: string,
    description: string,
    about: string,
    copyright: string,
    publicPath: string,
    extra: object
}
export interface BbobJSApi {
    blog: {
        tags: string[],
        categories: string[],
        links: LinkInfo[],
        nextFileLinks: string[]
    },
    meta: BbobMeta
    api: {
        resetNextLinkInfosOffset(): void,
        resetNextLinkInfosOffset(new_offset: number): void,
        nextLinkInfos(callback: LinkInfoArrayProcessCallBack): void,
        getArticleFromAddress(address: string, callback: ArticleProcessCallBack): void,
        getLinkInfosWithTag(tagName: string, callback: LinkInfoArrayProcessCallBack): void,
        getLinkInfosWithCategory(categoryName: string, callback: LinkInfoArrayProcessCallBack): void
    }
}

export declare var Bbob: BbobJSApi
export default Bbob;