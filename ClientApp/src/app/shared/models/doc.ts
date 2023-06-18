import { BehaviorSubject } from "rxjs";
import { Page } from "./page";

export interface Doc {
    name: string;
    directory: string;
    created: Date;
    lastModified: Date;
    pages: Page[];
    hasSelectedPages: BehaviorSubject<boolean>;
    downloadUrl?: string;
    canRevertChanges: BehaviorSubject<boolean>;
    corrupt: BehaviorSubject<boolean>;
    passwordProtected: BehaviorSubject<boolean>;
}
