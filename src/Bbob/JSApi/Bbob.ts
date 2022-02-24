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
export interface FilterSource {
    text: string,
    address: string
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
    blogCountOneTime: number,
    allLink: string,
    baseUrl: string,
    extra: object
}
export interface BbobJSApi {
    blog: {
        categories: FilterSource[],
        tags: FilterSource[],
        archives: FilterSource[],
        links: LinkInfo[],
        nextFileLinks: string[]
    },
    meta: BbobMeta
    api: {
        resetNextLinkInfosOffset(): void,
        resetNextLinkInfosOffset(new_offset: number): void,
        nextLinkInfos(callback: LinkInfoArrayProcessCallBack): void,
        getArticleFromAddress(address: string, callback: ArticleProcessCallBack): void,
        getLinkInfosWithAddress(address: string, callback: LinkInfoArrayProcessCallBack): void,
    }
}

export declare var Bbob: BbobJSApi
export default Bbob;