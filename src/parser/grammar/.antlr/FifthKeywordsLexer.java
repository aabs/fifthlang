// Generated from c:\dev\fifthlang\fifth.parser\grammar\FifthKeywords.g4 by ANTLR 4.8
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class FifthKeywordsLexer extends Lexer {
	static { RuntimeMetaData.checkVersion("4.8", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		ALIAS=1, AS=2, ELSE=3, IF=4, WHILE=5, NEW=6, WITH=7, RETURN=8, USE=9, 
		TRUE=10, FALSE=11, LIST=12;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"ALIAS", "AS", "ELSE", "IF", "WHILE", "NEW", "WITH", "RETURN", "USE", 
			"TRUE", "FALSE", "LIST"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'alias'", "'as'", "'else'", "'if'", "'while'", "'new'", "'with'", 
			"'return'", "'use'", "'true'", "'false'", "'list'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "ALIAS", "AS", "ELSE", "IF", "WHILE", "NEW", "WITH", "RETURN", 
			"USE", "TRUE", "FALSE", "LIST"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}


	public FifthKeywordsLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "FifthKeywords.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public String[] getChannelNames() { return channelNames; }

	@Override
	public String[] getModeNames() { return modeNames; }

	@Override
	public ATN getATN() { return _ATN; }

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2\16V\b\1\4\2\t\2\4"+
		"\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13\t"+
		"\13\4\f\t\f\4\r\t\r\3\2\3\2\3\2\3\2\3\2\3\2\3\3\3\3\3\3\3\4\3\4\3\4\3"+
		"\4\3\4\3\5\3\5\3\5\3\6\3\6\3\6\3\6\3\6\3\6\3\7\3\7\3\7\3\7\3\b\3\b\3\b"+
		"\3\b\3\b\3\t\3\t\3\t\3\t\3\t\3\t\3\t\3\n\3\n\3\n\3\n\3\13\3\13\3\13\3"+
		"\13\3\13\3\f\3\f\3\f\3\f\3\f\3\f\3\r\3\r\3\r\3\r\3\r\2\2\16\3\3\5\4\7"+
		"\5\t\6\13\7\r\b\17\t\21\n\23\13\25\f\27\r\31\16\3\2\2\2U\2\3\3\2\2\2\2"+
		"\5\3\2\2\2\2\7\3\2\2\2\2\t\3\2\2\2\2\13\3\2\2\2\2\r\3\2\2\2\2\17\3\2\2"+
		"\2\2\21\3\2\2\2\2\23\3\2\2\2\2\25\3\2\2\2\2\27\3\2\2\2\2\31\3\2\2\2\3"+
		"\33\3\2\2\2\5!\3\2\2\2\7$\3\2\2\2\t)\3\2\2\2\13,\3\2\2\2\r\62\3\2\2\2"+
		"\17\66\3\2\2\2\21;\3\2\2\2\23B\3\2\2\2\25F\3\2\2\2\27K\3\2\2\2\31Q\3\2"+
		"\2\2\33\34\7c\2\2\34\35\7n\2\2\35\36\7k\2\2\36\37\7c\2\2\37 \7u\2\2 \4"+
		"\3\2\2\2!\"\7c\2\2\"#\7u\2\2#\6\3\2\2\2$%\7g\2\2%&\7n\2\2&\'\7u\2\2\'"+
		"(\7g\2\2(\b\3\2\2\2)*\7k\2\2*+\7h\2\2+\n\3\2\2\2,-\7y\2\2-.\7j\2\2./\7"+
		"k\2\2/\60\7n\2\2\60\61\7g\2\2\61\f\3\2\2\2\62\63\7p\2\2\63\64\7g\2\2\64"+
		"\65\7y\2\2\65\16\3\2\2\2\66\67\7y\2\2\678\7k\2\289\7v\2\29:\7j\2\2:\20"+
		"\3\2\2\2;<\7t\2\2<=\7g\2\2=>\7v\2\2>?\7w\2\2?@\7t\2\2@A\7p\2\2A\22\3\2"+
		"\2\2BC\7w\2\2CD\7u\2\2DE\7g\2\2E\24\3\2\2\2FG\7v\2\2GH\7t\2\2HI\7w\2\2"+
		"IJ\7g\2\2J\26\3\2\2\2KL\7h\2\2LM\7c\2\2MN\7n\2\2NO\7u\2\2OP\7g\2\2P\30"+
		"\3\2\2\2QR\7n\2\2RS\7k\2\2ST\7u\2\2TU\7v\2\2U\32\3\2\2\2\3\2\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}