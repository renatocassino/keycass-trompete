import { debounce } from 'lodash';
import { Speaker } from './speaker';

const TIME_TO_DEBOUNCE = 1500;
const MAX_CHARS_BEFORE_SPEAK = 20;

const speaker = new Speaker();
const charsToBreak = new Set([' ',  ',', '.', '!', '?', '\n']);

export class TextProcessor {
    private currentText = [];
    private debounce = null;

    constructor() {
        this.debounce = debounce(this.speak.bind(this), TIME_TO_DEBOUNCE);
    }

    public getText(): string {
        return this.currentText.join('');
    }

    public push(letter: string) {
        this.currentText.push(letter);

        if (this.currentText.length > MAX_CHARS_BEFORE_SPEAK) {
            this.debounce.cancel()
            this.speak(true);
        }

        this.debounce();
    }

    async speak(forceToSpeak = false) {
        const expression = this.getExpression(forceToSpeak);
        console.log(`Expression: ${expression}`)

        if (expression.match(/[a-z]/)) {
            await speaker.speak(expression);
        } else {
            console.log(`Expression: ${expression}. Impossible to speak!`);
        }

        const store = (window as any).Alpine?.store('speech') as
            | { voiceURI: string; lastKey: string; currentText: string }
            | undefined;

        if (store) {
            store.lastKey = '';
            store.currentText = this.getText();
        }
    }

    getExpression(forceToSpeak = false): string {
        const currentExpression = this.currentText.join('').trim();

        // Procura do final para o início pelo último caractere de quebra
        if (forceToSpeak) {
            for (let i = currentExpression.length - 1; i >= 0; i--) {
                if (!charsToBreak.has(currentExpression[i])) {
                    continue;
                }

                // Encontrou um caractere de quebra
                const textToReturn = currentExpression.substring(0, i + 1); // Até o caractere (incluindo)
                const remaining = currentExpression.substring(i + 1); // Depois do caractere

                this.currentText = remaining.split('');

                if (remaining) {
                    this.debounce();
                }

                return textToReturn;
            }
        }

        this.currentText = [];

        return currentExpression;
    }
};
