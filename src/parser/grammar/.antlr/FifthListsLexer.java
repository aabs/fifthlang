// Generated from c:\dev\fifthlang\fifth.parser\grammar\FifthLists.g4 by ANTLR 4.8
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class FifthListsLexer extends Lexer {
	static { RuntimeMetaData.checkVersion("4.8", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		ASSIGN=1, CLOSEBRACK=2, CLOSEBRACE=3, CLOSEPAREN=4, COLON=5, COMMA=6, 
		DIVIDE=7, DOT=8, EQ=9, HASH=10, LAMBDASEP=11, MINUS=12, OPENBRACK=13, 
		OPENBRACE=14, OPENPAREN=15, PLUS=16, QMARK=17, TIMES=18, PERCENT=19, POWER=20, 
		NEQ=21, GT=22, LT=23, GEQ=24, LEQ=25, AMP=26, AND=27, OR=28, NOT=29, SEMICOLON=30, 
		IDENTIFIER=31, STRING=32, INT=33, FLOAT=34, WS=35;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"ASSIGN", "CLOSEBRACK", "CLOSEBRACE", "CLOSEPAREN", "COLON", "COMMA", 
			"DIVIDE", "DOT", "EQ", "HASH", "LAMBDASEP", "MINUS", "OPENBRACK", "OPENBRACE", 
			"OPENPAREN", "PLUS", "QMARK", "TIMES", "PERCENT", "POWER", "NEQ", "GT", 
			"LT", "GEQ", "LEQ", "AMP", "AND", "OR", "NOT", "SEMICOLON", "IDENTIFIER", 
			"LETTER", "STRING", "EXP", "DIGIT", "HEXDIGIT", "POSITIVEDIGIT", "INT", 
			"FLOAT", "WS"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'='", "']'", "'}'", "')'", "':'", "','", "'/'", "'.'", "'=='", 
			"'#'", "'=>'", "'-'", "'['", "'{'", "'('", "'+'", "'?'", "'*'", "'%'", 
			"'^'", "'!='", "'>'", "'<'", "'>='", "'<='", "'&'", "'&&'", "'||'", "'!'", 
			"';'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, "ASSIGN", "CLOSEBRACK", "CLOSEBRACE", "CLOSEPAREN", "COLON", "COMMA", 
			"DIVIDE", "DOT", "EQ", "HASH", "LAMBDASEP", "MINUS", "OPENBRACK", "OPENBRACE", 
			"OPENPAREN", "PLUS", "QMARK", "TIMES", "PERCENT", "POWER", "NEQ", "GT", 
			"LT", "GEQ", "LEQ", "AMP", "AND", "OR", "NOT", "SEMICOLON", "IDENTIFIER", 
			"STRING", "INT", "FLOAT", "WS"
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


	public FifthListsLexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "FifthLists.g4"; }

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
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2%\u00e3\b\1\4\2\t"+
		"\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4\13"+
		"\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22\t\22"+
		"\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31\t\31"+
		"\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t \4!"+
		"\t!\4\"\t\"\4#\t#\4$\t$\4%\t%\4&\t&\4\'\t\'\4(\t(\4)\t)\3\2\3\2\3\3\3"+
		"\3\3\4\3\4\3\5\3\5\3\6\3\6\3\7\3\7\3\b\3\b\3\t\3\t\3\n\3\n\3\n\3\13\3"+
		"\13\3\f\3\f\3\f\3\r\3\r\3\16\3\16\3\17\3\17\3\20\3\20\3\21\3\21\3\22\3"+
		"\22\3\23\3\23\3\24\3\24\3\25\3\25\3\26\3\26\3\26\3\27\3\27\3\30\3\30\3"+
		"\31\3\31\3\31\3\32\3\32\3\32\3\33\3\33\3\34\3\34\3\34\3\35\3\35\3\35\3"+
		"\36\3\36\3\37\3\37\3 \3 \5 \u0099\n \3 \3 \3 \7 \u009e\n \f \16 \u00a1"+
		"\13 \3!\3!\3\"\3\"\7\"\u00a7\n\"\f\"\16\"\u00aa\13\"\3\"\3\"\3\"\7\"\u00af"+
		"\n\"\f\"\16\"\u00b2\13\"\3\"\5\"\u00b5\n\"\3#\3#\5#\u00b9\n#\3#\3#\3$"+
		"\3$\3%\3%\3&\3&\3\'\3\'\5\'\u00c5\n\'\3\'\3\'\7\'\u00c9\n\'\f\'\16\'\u00cc"+
		"\13\'\5\'\u00ce\n\'\3(\5(\u00d1\n(\3(\3(\3(\6(\u00d6\n(\r(\16(\u00d7\3"+
		"(\5(\u00db\n(\3)\6)\u00de\n)\r)\16)\u00df\3)\3)\2\2*\3\3\5\4\7\5\t\6\13"+
		"\7\r\b\17\t\21\n\23\13\25\f\27\r\31\16\33\17\35\20\37\21!\22#\23%\24\'"+
		"\25)\26+\27-\30/\31\61\32\63\33\65\34\67\359\36;\37= ?!A\2C\"E\2G\2I\2"+
		"K\2M#O$Q%\3\2\13\4\2C\\c|\3\2$$\3\2))\4\2GGgg\4\2--//\3\2\62;\5\2\62;"+
		"CHch\3\2\63;\5\2\13\f\17\17\"\"\2\u00ec\2\3\3\2\2\2\2\5\3\2\2\2\2\7\3"+
		"\2\2\2\2\t\3\2\2\2\2\13\3\2\2\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21\3\2\2\2"+
		"\2\23\3\2\2\2\2\25\3\2\2\2\2\27\3\2\2\2\2\31\3\2\2\2\2\33\3\2\2\2\2\35"+
		"\3\2\2\2\2\37\3\2\2\2\2!\3\2\2\2\2#\3\2\2\2\2%\3\2\2\2\2\'\3\2\2\2\2)"+
		"\3\2\2\2\2+\3\2\2\2\2-\3\2\2\2\2/\3\2\2\2\2\61\3\2\2\2\2\63\3\2\2\2\2"+
		"\65\3\2\2\2\2\67\3\2\2\2\29\3\2\2\2\2;\3\2\2\2\2=\3\2\2\2\2?\3\2\2\2\2"+
		"C\3\2\2\2\2M\3\2\2\2\2O\3\2\2\2\2Q\3\2\2\2\3S\3\2\2\2\5U\3\2\2\2\7W\3"+
		"\2\2\2\tY\3\2\2\2\13[\3\2\2\2\r]\3\2\2\2\17_\3\2\2\2\21a\3\2\2\2\23c\3"+
		"\2\2\2\25f\3\2\2\2\27h\3\2\2\2\31k\3\2\2\2\33m\3\2\2\2\35o\3\2\2\2\37"+
		"q\3\2\2\2!s\3\2\2\2#u\3\2\2\2%w\3\2\2\2\'y\3\2\2\2){\3\2\2\2+}\3\2\2\2"+
		"-\u0080\3\2\2\2/\u0082\3\2\2\2\61\u0084\3\2\2\2\63\u0087\3\2\2\2\65\u008a"+
		"\3\2\2\2\67\u008c\3\2\2\29\u008f\3\2\2\2;\u0092\3\2\2\2=\u0094\3\2\2\2"+
		"?\u0098\3\2\2\2A\u00a2\3\2\2\2C\u00b4\3\2\2\2E\u00b6\3\2\2\2G\u00bc\3"+
		"\2\2\2I\u00be\3\2\2\2K\u00c0\3\2\2\2M\u00cd\3\2\2\2O\u00d0\3\2\2\2Q\u00dd"+
		"\3\2\2\2ST\7?\2\2T\4\3\2\2\2UV\7_\2\2V\6\3\2\2\2WX\7\177\2\2X\b\3\2\2"+
		"\2YZ\7+\2\2Z\n\3\2\2\2[\\\7<\2\2\\\f\3\2\2\2]^\7.\2\2^\16\3\2\2\2_`\7"+
		"\61\2\2`\20\3\2\2\2ab\7\60\2\2b\22\3\2\2\2cd\7?\2\2de\7?\2\2e\24\3\2\2"+
		"\2fg\7%\2\2g\26\3\2\2\2hi\7?\2\2ij\7@\2\2j\30\3\2\2\2kl\7/\2\2l\32\3\2"+
		"\2\2mn\7]\2\2n\34\3\2\2\2op\7}\2\2p\36\3\2\2\2qr\7*\2\2r \3\2\2\2st\7"+
		"-\2\2t\"\3\2\2\2uv\7A\2\2v$\3\2\2\2wx\7,\2\2x&\3\2\2\2yz\7\'\2\2z(\3\2"+
		"\2\2{|\7`\2\2|*\3\2\2\2}~\7#\2\2~\177\7?\2\2\177,\3\2\2\2\u0080\u0081"+
		"\7@\2\2\u0081.\3\2\2\2\u0082\u0083\7>\2\2\u0083\60\3\2\2\2\u0084\u0085"+
		"\7@\2\2\u0085\u0086\7?\2\2\u0086\62\3\2\2\2\u0087\u0088\7>\2\2\u0088\u0089"+
		"\7?\2\2\u0089\64\3\2\2\2\u008a\u008b\7(\2\2\u008b\66\3\2\2\2\u008c\u008d"+
		"\7(\2\2\u008d\u008e\7(\2\2\u008e8\3\2\2\2\u008f\u0090\7~\2\2\u0090\u0091"+
		"\7~\2\2\u0091:\3\2\2\2\u0092\u0093\7#\2\2\u0093<\3\2\2\2\u0094\u0095\7"+
		"=\2\2\u0095>\3\2\2\2\u0096\u0099\5A!\2\u0097\u0099\7a\2\2\u0098\u0096"+
		"\3\2\2\2\u0098\u0097\3\2\2\2\u0099\u009f\3\2\2\2\u009a\u009e\5A!\2\u009b"+
		"\u009e\5G$\2\u009c\u009e\7\60\2\2\u009d\u009a\3\2\2\2\u009d\u009b\3\2"+
		"\2\2\u009d\u009c\3\2\2\2\u009e\u00a1\3\2\2\2\u009f\u009d\3\2\2\2\u009f"+
		"\u00a0\3\2\2\2\u00a0@\3\2\2\2\u00a1\u009f\3\2\2\2\u00a2\u00a3\t\2\2\2"+
		"\u00a3B\3\2\2\2\u00a4\u00a8\7$\2\2\u00a5\u00a7\n\3\2\2\u00a6\u00a5\3\2"+
		"\2\2\u00a7\u00aa\3\2\2\2\u00a8\u00a6\3\2\2\2\u00a8\u00a9\3\2\2\2\u00a9"+
		"\u00ab\3\2\2\2\u00aa\u00a8\3\2\2\2\u00ab\u00b5\7$\2\2\u00ac\u00b0\7)\2"+
		"\2\u00ad\u00af\n\4\2\2\u00ae\u00ad\3\2\2\2\u00af\u00b2\3\2\2\2\u00b0\u00ae"+
		"\3\2\2\2\u00b0\u00b1\3\2\2\2\u00b1\u00b3\3\2\2\2\u00b2\u00b0\3\2\2\2\u00b3"+
		"\u00b5\7)\2\2\u00b4\u00a4\3\2\2\2\u00b4\u00ac\3\2\2\2\u00b5D\3\2\2\2\u00b6"+
		"\u00b8\t\5\2\2\u00b7\u00b9\t\6\2\2\u00b8\u00b7\3\2\2\2\u00b8\u00b9\3\2"+
		"\2\2\u00b9\u00ba\3\2\2\2\u00ba\u00bb\5M\'\2\u00bbF\3\2\2\2\u00bc\u00bd"+
		"\t\7\2\2\u00bdH\3\2\2\2\u00be\u00bf\t\b\2\2\u00bfJ\3\2\2\2\u00c0\u00c1"+
		"\t\t\2\2\u00c1L\3\2\2\2\u00c2\u00ce\7\62\2\2\u00c3\u00c5\7/\2\2\u00c4"+
		"\u00c3\3\2\2\2\u00c4\u00c5\3\2\2\2\u00c5\u00c6\3\2\2\2\u00c6\u00ca\5K"+
		"&\2\u00c7\u00c9\5G$\2\u00c8\u00c7\3\2\2\2\u00c9\u00cc\3\2\2\2\u00ca\u00c8"+
		"\3\2\2\2\u00ca\u00cb\3\2\2\2\u00cb\u00ce\3\2\2\2\u00cc\u00ca\3\2\2\2\u00cd"+
		"\u00c2\3\2\2\2\u00cd\u00c4\3\2\2\2\u00ceN\3\2\2\2\u00cf\u00d1\7/\2\2\u00d0"+
		"\u00cf\3\2\2\2\u00d0\u00d1\3\2\2\2\u00d1\u00d2\3\2\2\2\u00d2\u00d3\5M"+
		"\'\2\u00d3\u00d5\7\60\2\2\u00d4\u00d6\5G$\2\u00d5\u00d4\3\2\2\2\u00d6"+
		"\u00d7\3\2\2\2\u00d7\u00d5\3\2\2\2\u00d7\u00d8\3\2\2\2\u00d8\u00da\3\2"+
		"\2\2\u00d9\u00db\5E#\2\u00da\u00d9\3\2\2\2\u00da\u00db\3\2\2\2\u00dbP"+
		"\3\2\2\2\u00dc\u00de\t\n\2\2\u00dd\u00dc\3\2\2\2\u00de\u00df\3\2\2\2\u00df"+
		"\u00dd\3\2\2\2\u00df\u00e0\3\2\2\2\u00e0\u00e1\3\2\2\2\u00e1\u00e2\b)"+
		"\2\2\u00e2R\3\2\2\2\21\2\u0098\u009d\u009f\u00a8\u00b0\u00b4\u00b8\u00c4"+
		"\u00ca\u00cd\u00d0\u00d7\u00da\u00df\3\b\2\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}