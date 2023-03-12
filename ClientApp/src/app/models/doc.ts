import { Page } from "./page";

export interface Doc {
    name: string;
    directory: string;
    created: Date;
    lastModified: Date;
    pages: Page[];
    hasSelectedPages: boolean;
    downloadUrl?: string;
    canRevertChanges: boolean;
    corrupt: boolean;
    passwordProtected: boolean;
}
