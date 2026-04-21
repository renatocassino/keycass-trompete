import { debounce } from 'lodash';
import { Speaker } from './speaker';

const TIME_TO_DEBOUNCE = 1500;
const MAX_CHARS_BEFORE_SPEAK = 20;

const speaker = new Speaker();

export class TextProcessor {
    private currentText = [];
    private debounce = null;

    constructor() {
        this.debounce = debounce(this.speak.bind(this), TIME_TO_DEBOUNCE);
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
            return;
        }

        console.log(`Expression: ${expression}. Impossible to speak!`);
    }

    getExpression(forceToSpeak = false): string {
        const currentExpression = this.currentText.join('').trim();
        const charsToBreak = new Set([' ',  ',', '.', '!', '?', '\n']);

        // Procura do final para o início pelo último caractere de quebra
        if (forceToSpeak) {
            for (let i = currentExpression.length - 1; i >= 0; i--)
            {
                if (!charsToBreak.has(currentExpression[i]))
                {
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
